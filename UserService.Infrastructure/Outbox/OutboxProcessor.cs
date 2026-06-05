using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Kafka;

namespace UserService.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory m_scopeFactory;
    private readonly IProducer<string, string> m_producer;
    private readonly KafkaSettings m_kafkaSettings;
    private readonly ILogger<OutboxProcessor> m_logger;

    private static readonly TimeSpan s_pollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<OutboxProcessor> logger)
    {
        this.m_scopeFactory = scopeFactory;
        this.m_producer = producer;
        this.m_kafkaSettings = kafkaSettings.Value;
        this.m_logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                await this.ProcessBatchAsync(cancellation);
                await Task.Delay(s_pollingInterval, cancellation);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                this.m_logger.LogError(ex, "Outbox processor encountered an unexpected error");
                await Task.Delay(s_pollingInterval, CancellationToken.None);
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellation)
    {
        await using AsyncServiceScope scope = this.m_scopeFactory.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        List<OutboxMessage> messages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellation);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (OutboxMessage message in messages)
        {
            try
            {
                await this.m_producer.ProduceAsync(
                    this.m_kafkaSettings.Topic,
                    new Message<string, string>
                    {
                        Key = message.Id.ToString(),
                        Value = message.Payload
                    },
                    cancellation);

                message.MarkProcessed();
                this.m_logger.LogInformation("Published outbox message {MessageId} ({EventType})", message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                this.m_logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                message.MarkFailed(ex.Message);
            }
        }

        await db.SaveChangesAsync(cancellation);
    }
}
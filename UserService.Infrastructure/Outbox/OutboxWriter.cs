using System.Text.Json;
using UserService.Application.Abstractions;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Outbox;

public sealed class OutboxWriter : IOutboxWriter
{
    private readonly AppDbContext m_dbContext;

    public OutboxWriter(AppDbContext dbContext)
    {
        this.m_dbContext = dbContext;
    }

    public async Task WriteAsync<TEvent>(TEvent @event, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        string eventType = typeof(TEvent).Name;
        string payload = JsonSerializer.Serialize(@event);
        OutboxMessage message = OutboxMessage.Create(eventType, payload);

        await this.m_dbContext.OutboxMessages.AddAsync(message, cancellation);
    }
}
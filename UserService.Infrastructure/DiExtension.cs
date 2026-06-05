using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UserService.Application.Abstractions;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Kafka;
using UserService.Infrastructure.Outbox;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure;

public static class DiExtension
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("UserService")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBalanceHistoryRepository, BalanceHistoryRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));

        services.AddSingleton<IProducer<string, string>>(sp => {
            KafkaSettings settings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            ProducerConfig config = new ProducerConfig { BootstrapServers = settings.BootstrapServers };
            return new ProducerBuilder<string, string>(config).Build();
        });

        services.AddHostedService<OutboxProcessor>(); // ← отсутствовало

        return services;
    }
}
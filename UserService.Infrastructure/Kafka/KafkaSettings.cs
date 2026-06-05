namespace UserService.Infrastructure.Kafka;

public sealed class KafkaSettings
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = "users.events";
}
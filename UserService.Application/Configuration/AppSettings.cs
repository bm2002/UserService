namespace UserService.Application.Options;

public sealed class AppSettings
{
    public const string SectionName = "AppSettings";
    public decimal MaxBalance { get; set; }
    public bool UseDebezium { get; set; } 
}
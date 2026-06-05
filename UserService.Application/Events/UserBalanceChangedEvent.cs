namespace UserService.Application.Events;

public sealed class UserBalanceChangedEvent
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public decimal Delta { get; init; }
    public DateTime ChangedAt { get; init; }
}
namespace UserService.Domain.Entities;

public sealed class BalanceHistory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Delta { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private BalanceHistory() { }

    private BalanceHistory(Guid userId, decimal delta, decimal balanceAfter)
    {
        this.Id = Guid.NewGuid();
        this.UserId = userId;
        this.Delta = delta;
        this.BalanceAfter = balanceAfter;
        this.ChangedAt = DateTime.UtcNow;
    }

    public static BalanceHistory Create(Guid userId, decimal delta, decimal balanceAfter)
    {
        return new BalanceHistory(userId, delta, balanceAfter);
    }
}
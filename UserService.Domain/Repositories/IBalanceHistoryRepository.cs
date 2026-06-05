using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IBalanceHistoryRepository
{
    Task AddAsync(BalanceHistory history, CancellationToken cancellation);
    Task<IReadOnlyList<BalanceHistory>> GetRecentAsync(CancellationToken cancellation);
}
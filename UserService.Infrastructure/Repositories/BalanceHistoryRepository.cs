using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class BalanceHistoryRepository : IBalanceHistoryRepository
{
    private readonly AppDbContext m_dbContext;

    public BalanceHistoryRepository(AppDbContext dbContext)
    {
        this.m_dbContext = dbContext;
    }

    private const int Count = 20;
    public async Task AddAsync(BalanceHistory history, CancellationToken cancellation)
        => await this.m_dbContext.BalanceHistories.AddAsync(history, cancellation);

    public async Task<IReadOnlyList<BalanceHistory>> GetRecentAsync(CancellationToken cancellation)
        => await this.m_dbContext.BalanceHistories
            .AsNoTracking()
            .OrderByDescending(h => h.ChangedAt)
            .Take(Count)
            .ToListAsync(cancellation);
}
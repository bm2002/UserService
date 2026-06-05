using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext m_dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        this.m_dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellation)
        => this.m_dbContext.SaveChangesAsync(cancellation);
}

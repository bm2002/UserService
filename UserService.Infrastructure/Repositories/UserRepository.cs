using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext m_dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        this.m_dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellation)
        => await this.m_dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellation);

    /// <summary>
    /// bulk get users by ids, used for batch processing of transactions, to minimize the number of database calls
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellation)
        => await this.m_dbContext.Users.Where(u => ids.Contains(u.Id)).ToListAsync(cancellation);

    public async Task AddAsync(User user, CancellationToken cancellation)
        => await this.m_dbContext.Users.AddAsync(user, cancellation);
}
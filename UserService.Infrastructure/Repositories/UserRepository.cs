using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellation)
        => await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellation);

    /// <summary>
    /// bulk get users by ids, used for batch processing of transactions, to minimize the number of database calls
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellation)
        => await dbContext.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).ToListAsync(cancellation);

    public async Task AddAsync(User user, CancellationToken cancellation)
        => await dbContext.Users.AddAsync(user, cancellation);
}
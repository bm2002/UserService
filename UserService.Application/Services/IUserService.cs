using UserService.Application.Dtos;
using UserService.Domain.Entities;

namespace UserService.Application.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(UserDto dto, CancellationToken cancellation);
    Task UpdateBalanceAsync(UpdateBalanceDto dto, CancellationToken cancellation);
    Task UpdateBalanceBulkAsync(IEnumerable<UpdateBalanceDto> dtos, CancellationToken cancellation);
}
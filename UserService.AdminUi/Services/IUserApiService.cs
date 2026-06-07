using UserService.AdminUi.Models;

namespace UserService.AdminUi.Services;

public interface IUserApiService
{
    Task<UserResponse?> CreateUserAsync(UserDto dto, CancellationToken cancellation);
    Task<string?> UpdateBalanceAsync(UpdateBalanceDto dto, CancellationToken cancellation);
    Task<IReadOnlyList<BalanceHistoryDto>> GetBalanceHistoryAsync(CancellationToken cancellation);
}
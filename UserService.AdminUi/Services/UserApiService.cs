using System.Net.Http.Json;
using UserService.AdminUi.Models;

namespace UserService.AdminUi.Services;

public sealed class UserApiService : IUserApiService
{
    private readonly HttpClient m_httpClient;

    public UserApiService(HttpClient httpClient)
    {
        this.m_httpClient = httpClient;
    }

    public async Task<UserResponse?> CreateUserAsync(UserDto dto, CancellationToken cancellation)
    {
        HttpResponseMessage response = await this.m_httpClient.PostAsJsonAsync("api/users", dto, cancellation);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken: cancellation);
    }

    public async Task<string?> UpdateBalanceAsync(UpdateBalanceDto dto, CancellationToken cancellation)
    {
        HttpResponseMessage response = await this.m_httpClient.PatchAsJsonAsync(
            $"api/users/{dto.UserId}/balance",
            dto.Delta,
            cancellation);

        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<string>(cancellationToken: cancellation)
               ?? await response.Content.ReadAsStringAsync(cancellation);
    }

    public async Task<IReadOnlyList<BalanceHistoryDto>> GetBalanceHistoryAsync(CancellationToken cancellation)
    {
        return await this.m_httpClient.GetFromJsonAsync<List<BalanceHistoryDto>>(
                   "api/users/balance/history",
                   cancellationToken: cancellation)
               ?? [];
    }
}
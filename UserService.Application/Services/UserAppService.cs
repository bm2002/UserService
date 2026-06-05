using Microsoft.Extensions.Options;
using UserService.Application.Dtos;
using UserService.Application.Options;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public sealed class UserAppService : IUserService
{
    private readonly IUserRepository m_userRepository;
    private readonly IBalanceHistoryRepository m_balanceHistoryRepository;
    private readonly IUnitOfWork m_unitOfWork;
    private readonly IOptions<AppSettings> m_settings;

    public UserAppService(
        IUserRepository userRepository,
        IBalanceHistoryRepository balanceHistoryRepository,
        IUnitOfWork unitOfWork,
        IOptions<AppSettings> settings)
    {
        this.m_userRepository = userRepository;
        this.m_balanceHistoryRepository = balanceHistoryRepository;
        this.m_unitOfWork = unitOfWork;
        this.m_settings = settings;
    }

    public async Task<User> CreateUserAsync(UserDto dto, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        User user = User.Create(dto.FullName, dto.BirthDate, dto.BirthPlace);

        await this.m_userRepository.AddAsync(user, cancellation);
        await this.m_unitOfWork.SaveChangesAsync(cancellation);

        return user;
    }

    public async Task UpdateBalanceAsync(UpdateBalanceDto dto, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        User? user = await this.m_userRepository.GetByIdAsync(dto.UserId, cancellation);

        if (user is null)
        {
            throw new DomainException($"Пользователь {dto.UserId} не найден.");
        }

        user.UpdateBalance(dto.Delta, this.m_settings.Value.MaxBalance);

        BalanceHistory history = BalanceHistory.Create(user.Id, dto.Delta, user.Balance);
        await this.m_balanceHistoryRepository.AddAsync(history, cancellation);

        await this.m_unitOfWork.SaveChangesAsync(cancellation);
    }

    public async Task UpdateBalanceBulkAsync(IEnumerable<UpdateBalanceDto> dtos, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        List<UpdateBalanceDto> dtoList = dtos.ToList();
        List<Guid> ids = dtoList
            .Select(x => x.UserId)
            .Distinct()
            .ToList();
        IReadOnlyList<User> users = await this.m_userRepository.GetByIdsAsync(ids, cancellation);

        Dictionary<Guid, User> userMap = users.ToDictionary(u => u.Id);

        foreach (UpdateBalanceDto dto in dtoList)
        {
            if (!userMap.TryGetValue(dto.UserId, out User? user))
            {
                throw new DomainException($"Пользователь {dto.UserId} не найден.");
            }

            user.UpdateBalance(dto.Delta, this.m_settings.Value.MaxBalance);

            BalanceHistory history = BalanceHistory.Create(user.Id, dto.Delta, user.Balance);
            await this.m_balanceHistoryRepository.AddAsync(history, cancellation);
        }

        await this.m_unitOfWork.SaveChangesAsync(cancellation);
    }

    public async Task<IReadOnlyList<BalanceHistoryDto>> GetRecentBalanceHistoryAsync(CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        IReadOnlyList<BalanceHistory> history = await this.m_balanceHistoryRepository.GetRecentAsync(cancellation);

        List<Guid> userIds = history
            .Select(h => h.UserId)
            .Distinct()
            .ToList();

        IReadOnlyList<User> users = await this.m_userRepository.GetByIdsAsync(userIds, cancellation);
        Dictionary<Guid, string> userNames = users.ToDictionary(u => u.Id, u => u.FullName);

        return history
            .Select(h => new BalanceHistoryDto(
                h.UserId,
                userNames.TryGetValue(h.UserId, out string? name) ? name : "Unknown",
                h.Delta,
                h.BalanceAfter,
                h.ChangedAt))
            .ToList();
    }
}
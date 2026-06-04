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
    private readonly IUnitOfWork m_unitOfWork;
    private readonly IOptions<AppSettings> m_settings;

    public UserAppService(IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IOptions<AppSettings> settings)
    {
        this.m_userRepository = userRepository;
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
        }

        await this.m_unitOfWork.SaveChangesAsync(cancellation);
    }
}
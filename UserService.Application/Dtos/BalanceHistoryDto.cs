namespace UserService.Application.Dtos;

public sealed record BalanceHistoryDto(
    Guid UserId,
    string UserFullName,
    decimal Delta,
    decimal BalanceAfter,
    DateTime ChangedAt);
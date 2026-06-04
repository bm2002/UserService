namespace UserService.Application.Dtos;

public sealed record UpdateBalanceDto(Guid UserId, decimal Delta);
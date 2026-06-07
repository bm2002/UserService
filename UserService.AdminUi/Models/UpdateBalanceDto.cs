namespace UserService.AdminUi.Models;

public sealed record UpdateBalanceDto(Guid UserId, decimal Delta);
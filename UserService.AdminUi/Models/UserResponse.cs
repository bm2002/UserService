namespace UserService.AdminUi.Models;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    DateOnly BirthDate,
    string BirthPlace,
    decimal Balance);
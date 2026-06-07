namespace UserService.AdminUi.Models;

public sealed record UserDto(
    string FullName,
    DateOnly BirthDate,
    string BirthPlace);
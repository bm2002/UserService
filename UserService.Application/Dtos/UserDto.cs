namespace UserService.Application.Dtos;

public sealed record UserDto(
    string FullName,
    DateOnly BirthDate,
    string BirthPlace);
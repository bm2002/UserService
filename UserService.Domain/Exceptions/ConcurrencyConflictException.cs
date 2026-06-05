namespace UserService.Domain.Exceptions;

public sealed class ConcurrencyConflictException()
    : Exception("Конфликт параллельного доступа. Повторите операцию.");
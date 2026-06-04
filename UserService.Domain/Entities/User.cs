using System.ComponentModel.DataAnnotations;
using UserService.Domain.Exceptions;

namespace UserService.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string BirthPlace { get; private set; }
    public decimal Balance { get; private set; }

    [Timestamp]
    public uint RowVersion { get; private set; }

    private User(string fullName, DateOnly birthDate, string birthPlace)
    {
        Id = Guid.NewGuid();
        FullName = fullName;
        BirthDate = birthDate;
        BirthPlace = birthPlace;
        Balance = 0m;
    }

    public static User Create(string fullName, DateOnly birthDate, string birthPlace)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("FullName is required");
        }

        if (string.IsNullOrWhiteSpace(birthPlace))
        {
            throw new DomainException("BirthPlace is required");
        }

        return new User(fullName, birthDate, birthPlace);
    }

    public void UpdateBalance(decimal delta, decimal maxBalance)
    {
        decimal newBalance = this.Balance + delta;

        if (newBalance < 0)
        {
            throw new DomainException("Баланс не может быть отрицательным.");
        }

        if (newBalance > maxBalance)
        {
            throw new DomainException($"Баланс не может превышать {maxBalance}.");
        }

        this.Balance = newBalance;
    }
}
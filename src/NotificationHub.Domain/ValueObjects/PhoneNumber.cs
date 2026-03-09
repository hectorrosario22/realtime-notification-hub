using NotificationHub.Domain.Common;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidPhoneNumberException("Phone number cannot be empty.");

        var cleaned = new string([.. phoneNumber.Where(c => char.IsDigit(c) || c == '+')]);

        if (cleaned.Length < 10 || cleaned.Length > 15)
            throw new InvalidPhoneNumberException($"'{phoneNumber}' is not a valid phone number.");

        return new PhoneNumber(cleaned);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}

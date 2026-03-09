using NotificationHub.Domain.Common;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Domain.ValueObjects;

public sealed class EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailAddressException("Email address cannot be empty.");

        email = email.Trim().ToLowerInvariant();

        if (!email.Contains('@') || email.Length > 254)
            throw new InvalidEmailAddressException($"'{email}' is not a valid email address.");

        return new EmailAddress(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;
}

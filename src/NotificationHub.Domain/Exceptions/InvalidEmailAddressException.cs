namespace NotificationHub.Domain.Exceptions;

public sealed class InvalidEmailAddressException(string message) : DomainException(message)
{
}

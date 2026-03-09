namespace NotificationHub.Domain.Exceptions;

public sealed class InvalidPhoneNumberException(string message) : DomainException(message)
{
}

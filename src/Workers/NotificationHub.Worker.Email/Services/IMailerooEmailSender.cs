namespace NotificationHub.Worker.Email.Services;

public interface IMailerooEmailSender
{
    Task<MailerooSendResult> SendAsync(MailerooSendRequest request, CancellationToken ct = default);
}

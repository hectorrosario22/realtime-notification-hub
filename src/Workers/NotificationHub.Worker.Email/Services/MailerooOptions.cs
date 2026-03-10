namespace NotificationHub.Worker.Email.Services;

public sealed class MailerooOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultFromAddress { get; set; } = string.Empty;
    public string DefaultFromName { get; set; } = string.Empty;
}

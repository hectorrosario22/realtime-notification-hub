using System.Net.Http.Json;
using System.Text.Json;

namespace NotificationHub.Worker.Email.Services;

public sealed class MailerooEmailSender(
    HttpClient httpClient,
    IHttpClientFactory httpClientFactory,
    ILogger<MailerooEmailSender> logger) : IMailerooEmailSender
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<MailerooSendResult> SendAsync(MailerooSendRequest request, CancellationToken ct = default)
    {
        // Resolve attachment content (download URLs → base64)
        List<MailerooAttachment>? attachments = null;
        if (request.Attachments is { Count: > 0 })
        {
            attachments = [];
            var downloader = httpClientFactory.CreateClient("AttachmentDownloader");

            foreach (var attachment in request.Attachments)
            {
                // Attachments are passed with Content already set to the URL;
                // we use FileName as a hint. The consumer builds MailerooAttachment
                // with Content = URL when it is a URL, or base64 directly if already encoded.
                // We detect by checking if content starts with "http".
                if (attachment.Content.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var response = await downloader.GetAsync(attachment.Content, ct);
                        response.EnsureSuccessStatusCode();

                        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                        var base64 = Convert.ToBase64String(bytes);

                        var contentType = response.Content.Headers.ContentType?.MediaType
                            ?? InferContentType(attachment.FileName);

                        attachments.Add(attachment with { Content = base64, ContentType = contentType });
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "Failed to download attachment {FileName} from {Url}",
                            attachment.FileName, attachment.Content);
                        // Skip failed attachments rather than failing the entire send
                    }
                }
                else
                {
                    attachments.Add(attachment);
                }
            }
        }

        var payload = attachments is not null
            ? request with { Attachments = attachments }
            : request;

        var httpResponse = await httpClient.PostAsJsonAsync("emails", payload, JsonOptions, ct);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);
            logger.LogError(
                "Maileroo API returned {StatusCode}: {Body}",
                (int)httpResponse.StatusCode, errorBody);
            return new MailerooSendResult(false, $"HTTP {(int)httpResponse.StatusCode}: {errorBody}", null);
        }

        var result = await httpResponse.Content.ReadFromJsonAsync<MailerooSendResponse>(JsonOptions, ct);
        if (result is null)
            return new MailerooSendResult(false, "Empty response from Maileroo", null);

        return new MailerooSendResult(result.Success, result.Message, result.Data?.ReferenceId);
    }

    private static string InferContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}

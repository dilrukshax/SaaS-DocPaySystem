using NotificationService.Application.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationService.Infrastructure.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IConfiguration _configuration;

    public EmailNotificationService(
        ISendGridClient sendGridClient,
        ILogger<EmailNotificationService> logger,
        IConfiguration configuration)
    {
        _sendGridClient = sendGridClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<NotificationResult> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(
                _configuration["SendGrid:FromEmail"],
                _configuration["SendGrid:FromName"]);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent ?? ConvertHtmlToPlainText(htmlContent),
                htmlContent);

            // Add metadata as custom headers
            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    msg.Headers.Add($"X-Metadata-{kvp.Key}", kvp.Value?.ToString() ?? "");
                }
            }

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return new NotificationResult(
                    IsSuccess: true,
                    MessageId: response.Headers.GetValues("X-Message-Id").FirstOrDefault(),
                    Status: "sent",
                    SentAt: DateTime.UtcNow,
                    ErrorMessage: null);
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email to {Email}. Status: {Status}, Body: {Body}",
                    toEmail, response.StatusCode, errorBody);

                return new NotificationResult(
                    IsSuccess: false,
                    MessageId: null,
                    Status: "failed",
                    SentAt: null,
                    ErrorMessage: $"SendGrid error: {response.StatusCode} - {errorBody}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
            return new NotificationResult(
                IsSuccess: false,
                MessageId: null,
                Status: "failed",
                SentAt: null,
                ErrorMessage: ex.Message);
        }
    }

    public async Task<NotificationResult> SendTemplateEmailAsync(
        string toEmail,
        string templateId,
        Dictionary<string, object> templateData,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(
                _configuration["SendGrid:FromEmail"],
                _configuration["SendGrid:FromName"]);
            var to = new EmailAddress(toEmail);

            var msg = new SendGridMessage()
            {
                From = from,
                TemplateId = templateId
            };

            msg.AddTo(to);

            // Add template data
            msg.SetTemplateData(templateData);

            // Add metadata as custom headers
            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    msg.Headers.Add($"X-Metadata-{kvp.Key}", kvp.Value?.ToString() ?? "");
                }
            }

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Template email sent successfully to {Email} using template {TemplateId}",
                    toEmail, templateId);

                return new NotificationResult(
                    IsSuccess: true,
                    MessageId: response.Headers.GetValues("X-Message-Id").FirstOrDefault(),
                    Status: "sent",
                    SentAt: DateTime.UtcNow,
                    ErrorMessage: null);
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send template email to {Email}. Status: {Status}, Body: {Body}",
                    toEmail, response.StatusCode, errorBody);

                return new NotificationResult(
                    IsSuccess: false,
                    MessageId: null,
                    Status: "failed",
                    SentAt: null,
                    ErrorMessage: $"SendGrid template error: {response.StatusCode} - {errorBody}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending template email to {Email}", toEmail);
            return new NotificationResult(
                IsSuccess: false,
                MessageId: null,
                Status: "failed",
                SentAt: null,
                ErrorMessage: ex.Message);
        }
    }

    private static string ConvertHtmlToPlainText(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Simple HTML to plain text conversion
        // In a real implementation, you might want to use a library like HtmlAgilityPack
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty)
            .Replace("&nbsp;", " ")
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Trim();
    }
}

public record NotificationResult(
    bool IsSuccess,
    string? MessageId,
    string Status,
    DateTime? SentAt,
    string? ErrorMessage);

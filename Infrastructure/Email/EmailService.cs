using Application.Common.Interfaces.Services;
using Infrastructure.Persistence;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailConfiguration _config;
    private readonly PlatformDbContext _db;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        EmailConfiguration config,
        PlatformDbContext db,
        ILogger<EmailService> logger)
    {
        _config = config;
        _db = db;
        _logger = logger;
    }

    public async Task SendAsync(Guid recipientUserId, string subject, string body,
        CancellationToken ct = default)
    {
        var email = await _db.Users.AsNoTracking()
            .Where(u => u.Id == recipientUserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning(
                "Skipping email '{Subject}' because user {UserId} has no email address.",
                subject, recipientUserId);
            return;
        }

        await SendToEmailAsync(email, subject, body, ct);
    }

    public async Task SendToEmailAsync(string recipientEmail, string subject, string body,
        CancellationToken ct = default) => await SendMessageAsync(recipientEmail, subject, body, null, null, ct);

    public async Task SendToEmailWithAttachmentAsync(
        string recipientEmail, string subject, string body, string attachmentName, byte[] attachment,
        CancellationToken ct = default) => await SendMessageAsync(recipientEmail, subject, body, attachmentName, attachment, ct);

    private async Task SendMessageAsync(string recipientEmail, string subject, string body,
        string? attachmentName, byte[]? attachment, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email is required.", nameof(recipientEmail));

        var fromAddress = string.IsNullOrWhiteSpace(_config.FromAddress)
            ? _config.SmtpUsername
            : _config.FromAddress;

        if (string.IsNullOrWhiteSpace(_config.SmtpServer) || string.IsNullOrWhiteSpace(fromAddress))
        {
            _logger.LogWarning(
                "Email '{Subject}' to {Recipient} was not sent because SMTP is not configured.",
                subject, recipientEmail);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config.FromName, fromAddress));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = subject;

            if (!string.IsNullOrWhiteSpace(_config.SupportEmail))
                message.ReplyTo.Add(new MailboxAddress(_config.FromName, _config.SupportEmail));

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            if (attachment is { Length: > 0 } && !string.IsNullOrWhiteSpace(attachmentName))
                bodyBuilder.Attachments.Add(attachmentName, attachment, new ContentType("application", "pdf"));
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = Math.Clamp(_config.SmtpTimeoutMilliseconds, 1000, 60000);
            var secureSocket = _config.UseSsl
                ? _config.SmtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_config.SmtpServer, _config.SmtpPort, secureSocket, ct);

            if (!string.IsNullOrWhiteSpace(_config.SmtpUsername))
                await client.AuthenticateAsync(_config.SmtpUsername, _config.SmtpPassword, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email '{Subject}' sent to {Recipient}.", subject, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email '{Subject}' to {Recipient}.",
                subject, recipientEmail);
            throw;
        }
    }

    public async Task SendToPlatformAdminsAsync(string subject, string body, CancellationToken ct = default)
    {
        var emails = await _db.Users.AsNoTracking()
            .Where(user => user.IsPlatformAdmin && user.Email != null && user.Email != "")
            .Select(user => user.Email!)
            .Distinct()
            .ToListAsync(ct);

        if (emails.Count == 0)
        {
            _logger.LogWarning("No platform administrator email found; '{Subject}' was not sent.", subject);
            return;
        }

        foreach (var email in emails)
            await SendToEmailAsync(email, subject, body, ct);
    }

    public async Task SendToSchoolAdminsAsync(Guid schoolId, string subject, string body,
        CancellationToken ct = default)
    {
        var emails = await _db.UserSchoolMemberships.AsNoTracking()
            .Where(m => m.SchoolId == schoolId && m.IsActive && m.Role == "SchoolAdmin")
            .Join(_db.Users.AsNoTracking(),
                membership => membership.ApplicationUserId,
                user => user.Id,
                (_, user) => user.Email)
            .Where(email => email != null && email != "")
            .Distinct()
            .ToListAsync(ct);

        if (emails.Count == 0)
        {
            _logger.LogWarning(
                "No school administrator email found for school {SchoolId}; '{Subject}' was not sent.",
                schoolId, subject);
            return;
        }

        foreach (var email in emails)
            await SendToEmailAsync(email!, subject, body, ct);
    }
}

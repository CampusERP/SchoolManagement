namespace Application.Common.Interfaces.Services;

public interface IEmailService
{
    /// <summary>Sends to the email address belonging to an application user.</summary>
    Task SendAsync(Guid recipientUserId, string subject, string body,
        CancellationToken ct = default);

    /// <summary>Sends to a raw email address (for example a registration applicant).</summary>
    Task SendToEmailAsync(string recipientEmail, string subject, string body,
        CancellationToken ct = default);

    Task SendToEmailWithAttachmentAsync(string recipientEmail, string subject, string body,
        string attachmentName, byte[] attachment, CancellationToken ct = default);

    Task SendToPlatformAdminsAsync(string subject, string body, CancellationToken ct = default);

    /// <summary>Sends the same message to every active school administrator.</summary>
    Task SendToSchoolAdminsAsync(Guid schoolId, string subject, string body,
        CancellationToken ct = default);
}

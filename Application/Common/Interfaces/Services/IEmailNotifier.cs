namespace Application.Common.Interfaces.Services;

/// <summary>
/// Identifies the available HTML email templates. The matching files live in
/// <c>Infrastructure/Email/Templates</c>.
/// </summary>
public enum EmailTemplate
{
    PasswordReset,
    Welcome,
    SchoolRegistrationReceived,
    SchoolRegistrationApproved,
    SchoolRegistrationRejected,
    InvoiceIssued,
    PaymentReceived,
    SubscriptionSuspended,
    SubscriptionReactivated,
    SubscriptionCancelled,
    Notification
}

public enum EmailRecipientKind { User, Address, PlatformAdmins, SchoolAdmins }

public sealed record EmailDeliveryJob(
    EmailRecipientKind RecipientKind,
    string Recipient,
    EmailTemplate Template,
    IReadOnlyDictionary<string, string?> Model,
    string? AttachmentName = null,
    byte[]? Attachment = null);

public interface IEmailDeliveryQueue
{
    ValueTask EnqueueAsync(EmailDeliveryJob job, CancellationToken ct = default);
    IAsyncEnumerable<EmailDeliveryJob> ReadAllAsync(CancellationToken ct = default);
}

/// <summary>
/// Fire-and-forget email notifications. Delivery failures are logged and never
/// bubble up, so they cannot roll back an otherwise successful business operation.
/// </summary>
public interface IEmailNotifier
{
    /// <summary>Delivers notifications queued during the current command after its changes are saved.</summary>
    Task FlushAsync(CancellationToken ct = default);
    /// <summary>The public frontend URL, useful for building login and reset links.</summary>
    string AppBaseUrl { get; }

    Task SendAsync(
        Guid recipientUserId,
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        CancellationToken ct = default);

    Task SendToEmailAsync(
        string recipientEmail,
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        CancellationToken ct = default);

    Task SendToEmailWithAttachmentAsync(
        string recipientEmail,
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        string attachmentName,
        byte[] attachment,
        CancellationToken ct = default);

    Task NotifyPlatformAdminsAsync(
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        CancellationToken ct = default);

    Task SendToSchoolAdminsAsync(
        Guid schoolId,
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        CancellationToken ct = default);
}

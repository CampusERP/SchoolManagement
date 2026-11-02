using Application.Common.Interfaces.Services;

namespace Infrastructure.Email;

public sealed class EmailNotifier : IEmailNotifier
{
    private readonly IEmailDeliveryQueue _deliveryQueue;
    private readonly EmailConfiguration _config;
    private readonly List<EmailDeliveryJob> _pending = [];

    public EmailNotifier(
        IEmailDeliveryQueue deliveryQueue,
        EmailConfiguration config)
    {
        _deliveryQueue = deliveryQueue;
        _config = config;
    }

    public string AppBaseUrl => _config.AppBaseUrl.TrimEnd('/');

    public Task SendAsync(
        Guid recipientUserId,
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        CancellationToken ct = default) =>
        QueueSend(new(EmailRecipientKind.User, recipientUserId.ToString(), template, model));

    public Task SendToEmailAsync(
        string recipientEmail,
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        CancellationToken ct = default) =>
        QueueSend(new(EmailRecipientKind.Address, recipientEmail, template, model));

    public Task SendToEmailWithAttachmentAsync(
        string recipientEmail,
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        string attachmentName,
        byte[] attachment,
        CancellationToken ct = default) =>
        QueueSend(new(EmailRecipientKind.Address, recipientEmail, template, model, attachmentName, attachment));

    public Task NotifyPlatformAdminsAsync(
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        CancellationToken ct = default) =>
        QueueSend(new(EmailRecipientKind.PlatformAdmins, "", template, model));

    public Task SendToSchoolAdminsAsync(
        Guid schoolId,
        EmailTemplate template,
        IReadOnlyDictionary<string, string?> model,
        CancellationToken ct = default) =>
        QueueSend(new(EmailRecipientKind.SchoolAdmins, schoolId.ToString(), template, model));

    private Task QueueSend(EmailDeliveryJob job)
    {
        _pending.Add(job);
        return Task.CompletedTask;
    }

    public async Task FlushAsync(CancellationToken ct = default)
    {
        foreach (var job in _pending)
            await _deliveryQueue.EnqueueAsync(job, CancellationToken.None);
        _pending.Clear();
    }
}

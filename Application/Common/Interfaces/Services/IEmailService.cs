namespace Application.Common.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(Guid recipientUserId, string subject, string body,
        CancellationToken ct = default);
}

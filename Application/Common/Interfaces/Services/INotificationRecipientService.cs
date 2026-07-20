using Application.Features.Notifications.Commands;

namespace Application.Common.Interfaces.Services;

public interface INotificationRecipientService
{
    Task<List<Guid>> GetRecipientsAsync(NotificationScope scope, Guid? targetUserId, Guid? targetClassRoom, Guid? targetGrade, CancellationToken ct = default);
}

using Application.Common.Interfaces.Services;
using Application.Features.Notifications.Commands;

namespace Infrastructure.Persistence.Services;

public class NotificationRecipientService : INotificationRecipientService
{
    public Task<List<Guid>> GetRecipientsAsync(NotificationScope scope, Guid? targetUserId, Guid? targetClassRoom, Guid? targetGrade, CancellationToken ct = default)
    {
        return Task.FromResult(new List<Guid>());
    }
}

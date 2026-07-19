using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Notifications.Commands;

public class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserService  _user;

    public MarkNotificationReadCommandHandler(INotificationRepository repository, ICurrentUserService user)
    { _repository = repository; _user = user; }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await _repository.GetByIdAsync(request.NotificationId, ct);

        if (notification is null || notification.RecipientUserId != _user.UserId)
            return Result.Failure("Notification not found.");

        notification.MarkRead();
        _repository.Update(notification);
        return Result.Success();
    }
}

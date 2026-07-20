using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Notifications.Commands;

public class MarkAllNotificationsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserService  _user;

    public MarkAllNotificationsReadCommandHandler(INotificationRepository repository, ICurrentUserService user)
    { _repository = repository; _user = user; }

    public async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var unread = await _repository.GetUnreadForUserAsync(_user.UserId!.Value, ct);

        foreach (var n in unread) 
        {
            n.MarkRead();
            _repository.Update(n);
        }
        return Result.Success();
    }
}

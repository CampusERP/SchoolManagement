using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Notifications;
using MediatR;

namespace Application.Features.Notifications.Commands;

public class SendNotificationCommandHandler
    : IRequestHandler<SendNotificationCommand, Result<Guid>>
{
    private readonly INotificationBatchRepository _batchRepository;
    private readonly INotificationRecipientService _recipientService;
    private readonly ICurrentUserService _user;
    private readonly IOutboxService       _outbox;

    public SendNotificationCommandHandler(
        INotificationBatchRepository batchRepository,
        INotificationRecipientService recipientService,
        ICurrentUserService user,
        IOutboxService outbox)
    {
        _batchRepository = batchRepository;
        _recipientService = recipientService;
        _user  = user;
        _outbox = outbox;
    }

    public async Task<Result<Guid>> Handle(SendNotificationCommand request, CancellationToken ct)
    {
        var recipientUserIds = await _recipientService.GetRecipientsAsync(
            request.Scope, request.TargetUserId, request.TargetClassRoom, request.TargetGrade, ct);

        if (!recipientUserIds.Any())
            return Result.Failure<Guid>("No recipients found for the specified scope.");

        var batch = NotificationBatch.Create(
            request.SchoolId, request.TemplateId, _user.UserId!.Value,
            request.Subject, request.Body, request.Channel,
            request.Scope.ToString());

        foreach (var userId in recipientUserIds)
            batch.AddRecipient(userId);

        await _batchRepository.AddAsync(batch);

        _outbox.Publish(new Application.Common.Messages.DeliverNotificationBatchMessage(
            batch.Id, request.SchoolId));

        return Result.Success(batch.Id);
    }
}

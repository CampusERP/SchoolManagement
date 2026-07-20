using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Domain.Entities.Notifications;
using Domain.Entities.People;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Outbox;

public sealed class CreateSchoolAdminProfileHandler(ApplicationDbContext db) : IOutboxMessageHandler<CreateSchoolAdminProfileMessage>
{
    public async Task HandleAsync(CreateSchoolAdminProfileMessage message, CancellationToken ct)
    {
        var exists = await db.SchoolAdminProfiles.IgnoreQueryFilters()
            .AnyAsync(x => x.ApplicationUserId == message.UserId && x.SchoolId == message.SchoolId, ct);
        if (!exists) db.SchoolAdminProfiles.Add(SchoolAdminProfile.Create(message.SchoolId, message.UserId, message.FirstName, message.LastName));
        await db.SaveChangesAsync(ct);
    }
}

public sealed class LinkStudentLoginHandler(ApplicationDbContext db) : IOutboxMessageHandler<LinkStudentLoginMessage>
{
    public async Task HandleAsync(LinkStudentLoginMessage message, CancellationToken ct)
    {
        var student = await db.Students.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == message.StudentId, ct)
            ?? throw new InvalidOperationException($"Student {message.StudentId} was not found.");
        student.LinkLogin(message.ApplicationUserId);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class LinkTeacherLoginHandler(ApplicationDbContext db) : IOutboxMessageHandler<LinkTeacherLoginMessage>
{
    public async Task HandleAsync(LinkTeacherLoginMessage message, CancellationToken ct)
    {
        var teacher = await db.Teachers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == message.TeacherId, ct)
            ?? throw new InvalidOperationException($"Teacher {message.TeacherId} was not found.");
        teacher.LinkLogin(message.ApplicationUserId);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class LinkParentLoginHandler(ApplicationDbContext db) : IOutboxMessageHandler<LinkParentLoginMessage>
{
    public async Task HandleAsync(LinkParentLoginMessage message, CancellationToken ct)
    {
        var parent = await db.Parents.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == message.ParentId, ct)
            ?? throw new InvalidOperationException($"Parent {message.ParentId} was not found.");
        parent.LinkLogin(message.ApplicationUserId);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class DeliverNotificationBatchHandler(
    ApplicationDbContext db,
    IEmailService emailService,
    ILogger<DeliverNotificationBatchHandler> logger)
    : IOutboxMessageHandler<DeliverNotificationBatchMessage>
{
    public async Task HandleAsync(DeliverNotificationBatchMessage message, CancellationToken ct)
    {
        var batch = await db.NotificationBatches.IgnoreQueryFilters().Include(x => x.Notifications)
            .SingleOrDefaultAsync(x => x.Id == message.NotificationBatchId, ct)
            ?? throw new InvalidOperationException($"Notification batch {message.NotificationBatchId} was not found.");

        switch (batch.Channel)
        {
            case NotificationChannel.InApp:
                foreach (var notification in batch.Notifications.Where(x => x.Status == NotificationStatus.Pending))
                    notification.MarkDelivered();
                await db.SaveChangesAsync(ct);
                break;

            case NotificationChannel.Email:
                var pendingRecipients = batch.Notifications
                    .Where(x => x.Status == NotificationStatus.Pending)
                    .ToList();

                foreach (var notification in pendingRecipients)
                {
                    try
                    {
                        await emailService.SendAsync(
                            notification.RecipientUserId,
                            batch.Subject,
                            batch.Body,
                            ct);
                        notification.MarkDelivered();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send email to user {UserId} for batch {BatchId}",
                            notification.RecipientUserId, batch.Id);
                        notification.MarkFailed();
                    }
                }
                await db.SaveChangesAsync(ct);
                break;

            case NotificationChannel.SMS:
            case NotificationChannel.Push:
                throw new NotSupportedException($"{batch.Channel} notification delivery is not yet implemented.");
        }
    }
}

namespace Application.Common.Messages;

/// <summary>
/// Published when RegisterSchoolAdmin succeeds on PlatformDb.
/// OutboxProcessor creates SchoolAdminProfile in ApplicationDb.
/// </summary>
public record CreateSchoolAdminProfileMessage(
    Guid UserId,
    Guid SchoolId,
    string FirstName,
    string LastName);

/// <summary>
/// Published when CreateStudent creates a login on PlatformDb.
/// OutboxProcessor links ApplicationUserId to Student profile in ApplicationDb.
/// </summary>
public record LinkStudentLoginMessage(
    Guid StudentId,
    Guid ApplicationUserId);

/// <summary>
/// Published when CreateTeacher creates a login on PlatformDb.
/// OutboxProcessor links ApplicationUserId to Teacher profile in ApplicationDb.
/// </summary>
public record LinkTeacherLoginMessage(
    Guid TeacherId,
    Guid ApplicationUserId);

/// <summary>
/// Published when CreateParent creates a login on PlatformDb.
/// OutboxProcessor links ApplicationUserId to Parent profile in ApplicationDb.
/// </summary>
public record LinkParentLoginMessage(
    Guid ParentId,
    Guid ApplicationUserId);

/// <summary>
/// Published when a NotificationBatch is created.
/// OutboxProcessor actually delivers via email/SMS/push channel.
/// </summary>
public record DeliverNotificationBatchMessage(
    Guid NotificationBatchId,
    Guid SchoolId);

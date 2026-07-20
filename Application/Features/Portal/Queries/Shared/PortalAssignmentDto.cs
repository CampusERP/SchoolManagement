using Domain.Enums;

namespace Application.Features.Portal.Queries.Shared;

public record PortalAssignmentDto(
    Guid            AssignmentId,
    string          Title,
    string          SubjectName,
    string          TeacherName,
    DateTime        DueDate,
    decimal?        MaxScore,
    SubmissionStatus? SubmissionStatus,
    decimal?        Grade,
    string?         TeacherFeedback,
    DateTime?       SubmittedAt);

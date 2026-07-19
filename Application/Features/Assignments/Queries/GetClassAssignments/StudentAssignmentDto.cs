using Domain.Enums;

namespace Application.Features.Assignments.Queries.GetClassAssignments;

public record StudentAssignmentDto(
    Guid            AssignmentId,
    string          Title,
    string?         Instructions,
    DateTime        DueDate,
    decimal?        MaxScore,
    SubmissionStatus SubmissionStatus,
    decimal?        Grade,
    string?         Feedback,
    DateTime?       SubmittedAt,
    List<string>    AttachedFileUrls);

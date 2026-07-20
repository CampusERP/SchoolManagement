namespace Application.Features.Assignments.Queries.GetClassAssignments;

public record AssignmentSummaryDto(
    Guid     Id,
    string   Title,
    DateTime DueDate,
    decimal? MaxScore,
    int      TotalSubmissions,
    int      GradedSubmissions,
    int      PendingSubmissions);

namespace Application.Features.Exams.Commands;

public record StudentResultEntry(Guid StudentEnrollmentId, decimal Score, string? Remarks = null);
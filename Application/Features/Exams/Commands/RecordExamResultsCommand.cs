using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Exams.Commands;

public record RecordExamResultsCommand(Guid SchoolId, Guid ExamId,
    Guid ExamScheduleId, List<StudentResultEntry> Results)
    : ICommand, IBaseCommand, ITenantScopedRequest;

using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Exams.Commands;

public record LockExamCommand(Guid SchoolId, Guid ExamId)
    : ICommand, IBaseCommand, ITenantScopedRequest;

using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Exams.Commands;

public record CreateExamCommand(Guid SchoolId, Guid SubjectId, Guid TermId,
    string Name, decimal MaxScore)
    : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;

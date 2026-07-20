using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.CreateGradeLevel;

public record CreateGradeLevelCommand(
    Guid SchoolId,
    Guid EducationStageId,
    string Name,
    int Sequence) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.UpdateGradeLevel;

public record UpdateGradeLevelCommand(
    Guid SchoolId,
    Guid GradeLevelId,
    string Name,
    int Sequence) : ICommand, IBaseCommand, ITenantScopedRequest;

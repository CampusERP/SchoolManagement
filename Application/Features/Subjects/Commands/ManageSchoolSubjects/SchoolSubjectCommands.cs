using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Subjects.Commands.ManageSchoolSubjects;

public record CreateSchoolSubjectCommand(Guid SchoolId, Guid GradeLevelId, string Code, string Name, string? Description = null)
    : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;

public record UpdateSchoolSubjectCommand(Guid SchoolId, Guid CurriculumSubjectId, string Code, string Name, string? Description = null)
    : ICommand, IBaseCommand, ITenantScopedRequest;

public record DeleteSchoolSubjectCommand(Guid SchoolId, Guid SubjectId)
    : ICommand, IBaseCommand, ITenantScopedRequest;

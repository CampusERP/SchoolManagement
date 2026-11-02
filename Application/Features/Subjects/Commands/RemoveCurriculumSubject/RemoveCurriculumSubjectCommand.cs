using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Subjects.Commands.RemoveCurriculumSubject;

public record RemoveCurriculumSubjectCommand(Guid SchoolId, Guid CurriculumSubjectId)
    : ICommand, IBaseCommand, ITenantScopedRequest;

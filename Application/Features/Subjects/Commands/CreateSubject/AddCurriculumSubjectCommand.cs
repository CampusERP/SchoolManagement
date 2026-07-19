using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Features.Subjects.Commands.CreateSubject;

public record AddCurriculumSubjectCommand(Guid SchoolId, Guid GradeLevelId, Guid SubjectId)
    : ICommand<Guid>, ITenantScopedRequest;

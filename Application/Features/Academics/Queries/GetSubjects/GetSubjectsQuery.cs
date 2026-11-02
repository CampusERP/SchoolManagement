using Application.Common.Models;
using MediatR;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Queries.GetSubjects;

public record GetSubjectsQuery(Guid SchoolId, Guid? GradeLevelId = null)
    : IRequest<Result<List<SubjectDto>>>, ITenantScopedRequest;

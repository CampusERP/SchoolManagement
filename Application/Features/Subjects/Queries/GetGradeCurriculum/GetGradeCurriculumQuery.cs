using Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Subjects.Queries.GetGradeCurriculum;

public record GetGradeCurriculumQuery(Guid SchoolId, Guid GradeLevelId)
    : IRequest<Result<List<CurriculumSubjectDto>>>, ITenantScopedRequest;

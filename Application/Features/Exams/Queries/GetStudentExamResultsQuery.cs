using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public record GetStudentExamResultsQuery(Guid SchoolId, Guid StudentEnrollmentId,
    Guid? TermId = null, PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<ExamResultDto>>>, ITenantScopedRequest;

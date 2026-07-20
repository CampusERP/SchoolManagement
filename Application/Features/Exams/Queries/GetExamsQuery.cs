using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public record GetExamsQuery(Guid SchoolId, Guid TermId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<ExamListDto>>>, ITenantScopedRequest;

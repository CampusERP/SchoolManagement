using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public record GetReportCardsQuery(Guid SchoolId, Guid StudentEnrollmentId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<ReportCardDto>>>, ITenantScopedRequest;

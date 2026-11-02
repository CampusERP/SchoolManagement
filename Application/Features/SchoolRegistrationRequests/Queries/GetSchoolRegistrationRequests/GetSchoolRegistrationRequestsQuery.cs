using Application.Common.Models;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Queries.GetSchoolRegistrationRequests;

public record GetSchoolRegistrationRequestsQuery(
    string? Search = null,
    string? Status = null,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<SchoolRegistrationRequestDto>>>;

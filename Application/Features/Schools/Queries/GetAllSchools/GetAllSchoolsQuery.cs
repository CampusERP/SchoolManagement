using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Queries.GetAllSchools;

public record GetAllSchoolsQuery(
    PaginationParams? Pagination = null,
    string? Search = null)
    : IRequest<Result<PagedResult<SchoolListDto>>>;

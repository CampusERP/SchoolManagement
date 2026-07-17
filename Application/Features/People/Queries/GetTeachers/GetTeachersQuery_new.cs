using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetTeachers;

public record GetTeachersQuery(Guid SchoolId, string? SearchTerm = null,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<TeacherListDto>>>, ITenantScopedRequest;

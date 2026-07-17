using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetStudents;

public record GetStudentsQuery(Guid SchoolId, string? SearchTerm = null,
    Guid? GradeLevelId = null, Guid? ClassRoomId = null,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<StudentListDto>>>, ITenantScopedRequest;
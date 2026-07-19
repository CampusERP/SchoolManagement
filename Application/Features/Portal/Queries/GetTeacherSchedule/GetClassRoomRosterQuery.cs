using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public record GetClassRoomRosterQuery(
    Guid SchoolId,
    Guid ClassRoomId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<RosterStudentDto>>>, ITenantScopedRequest;

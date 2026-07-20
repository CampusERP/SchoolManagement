using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildAttendance;

public record GetParentChildAttendanceQuery(
    Guid SchoolId,
    Guid StudentId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<PortalAttendanceRecordDto>>>, ITenantScopedRequest;

public record PortalAttendanceRecordDto(
    DateOnly Date,
    string   SubjectName,
    string   Status,
    string?  Note);

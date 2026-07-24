using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetClassRooms;

public record GetClassRoomTeachingAssignmentsQuery(Guid SchoolId, Guid ClassRoomId)
    : IRequest<Result<List<ClassRoomTeachingAssignmentDto>>>, ITenantScopedRequest;

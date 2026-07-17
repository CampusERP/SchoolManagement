using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetRooms;

public record GetRoomsQuery(Guid SchoolId)
    : IRequest<Result<List<RoomDto>>>, ITenantScopedRequest;

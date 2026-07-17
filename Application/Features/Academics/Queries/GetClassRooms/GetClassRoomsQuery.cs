using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetClassRooms;

public record GetClassRoomsQuery(Guid SchoolId, Guid? AcademicYearId = null,
    Guid? GradeLevelId = null)
    : IRequest<Result<List<ClassRoomDetailDto>>>, ITenantScopedRequest;

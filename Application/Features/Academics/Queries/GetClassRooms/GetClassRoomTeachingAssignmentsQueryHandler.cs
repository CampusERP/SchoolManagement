using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetClassRooms;

public class GetClassRoomTeachingAssignmentsQueryHandler
    : IRequestHandler<GetClassRoomTeachingAssignmentsQuery, Result<List<ClassRoomTeachingAssignmentDto>>>
{
    private readonly IAcademicReadService _academicReadService;
    public GetClassRoomTeachingAssignmentsQueryHandler(IAcademicReadService academicReadService) => _academicReadService = academicReadService;

    public async Task<Result<List<ClassRoomTeachingAssignmentDto>>> Handle(
        GetClassRoomTeachingAssignmentsQuery request, CancellationToken ct)
    {
        var items = await _academicReadService.GetClassRoomTeachingAssignmentsAsync(request.ClassRoomId, ct);
        return Result.Success(items);
    }
}

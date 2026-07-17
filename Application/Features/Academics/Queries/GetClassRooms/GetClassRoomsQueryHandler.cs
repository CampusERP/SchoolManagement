using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetClassRooms;

public class GetClassRoomsQueryHandler
    : IRequestHandler<GetClassRoomsQuery, Result<List<ClassRoomDetailDto>>>
{
    private readonly IAcademicReadService _academicReadService;
    public GetClassRoomsQueryHandler(IAcademicReadService academicReadService) => _academicReadService = academicReadService;

    public async Task<Result<List<ClassRoomDetailDto>>> Handle(
        GetClassRoomsQuery request, CancellationToken ct)
    {
        var items = await _academicReadService.GetClassRoomsAsync(request.AcademicYearId, request.GradeLevelId, ct);
        return Result.Success(items);
    }
}

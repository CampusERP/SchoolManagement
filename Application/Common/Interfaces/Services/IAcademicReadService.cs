using Application.Features.Academics.Queries.GetAcademicYears;
using Application.Features.Academics.Queries.GetClassRooms;
using Application.Features.Academics.Queries.GetGradeLevels;
using Application.Features.Academics.Queries.GetRooms;

namespace Application.Common.Interfaces.Services;

public interface IAcademicReadService
{
    Task<List<AcademicYearDto>> GetAcademicYearsAsync(CancellationToken ct = default);
    Task<List<GradeLevelDto>> GetGradeLevelsAsync(CancellationToken ct = default);
    Task<List<ClassRoomDetailDto>> GetClassRoomsAsync(Guid? academicYearId, Guid? gradeLevelId, CancellationToken ct = default);
    Task<List<RoomDto>> GetRoomsAsync(CancellationToken ct = default);
}

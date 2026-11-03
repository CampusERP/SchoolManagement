using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.Commands.CreateClassRoom;

public class CreateClassRoomCommandHandler : IRequestHandler<CreateClassRoomCommand, Result<Guid>>
{
    private readonly IClassRoomRepository _classRooms;
    private readonly IGradeLevelRepository _gradeLevels;
    private readonly IAcademicYearRepository _academicYears;
    private readonly IRoomRepository _rooms;
    private readonly ISchoolRepository _schools;

    public CreateClassRoomCommandHandler(
        IClassRoomRepository classRooms,
        IGradeLevelRepository gradeLevels,
        IAcademicYearRepository academicYears,
        IRoomRepository rooms,
        ISchoolRepository schools)
    {
        _classRooms = classRooms;
        _gradeLevels = gradeLevels;
        _academicYears = academicYears;
        _rooms = rooms;
        _schools = schools;
    }

    public async Task<Result<Guid>> Handle(CreateClassRoomCommand request, CancellationToken ct)
    {
        var campuses = await _schools.GetCampusesAsync(request.SchoolId, ct);
        if (campuses.Count > 0 && !request.CampusId.HasValue)
            return Result.Failure<Guid>("Select a campus for this classroom.");
        if (request.CampusId.HasValue && await _schools.GetCampusAsync(request.SchoolId, request.CampusId.Value, ct) is null)
            return Result.Failure<Guid>("The selected campus was not found in this school.");
        var gradeLevel = await _gradeLevels.GetByIdAsync(request.GradeLevelId, ct);
        if (gradeLevel is null)
            return Result.Failure<Guid>($"GradeLevel with ID '{request.GradeLevelId}' was not found.");

        var academicYear = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (academicYear is null)
            return Result.Failure<Guid>($"AcademicYear with ID '{request.AcademicYearId}' was not found.");

        if (request.RoomId.HasValue)
        {
            var room = await _rooms.GetByIdAsync(request.RoomId.Value, ct);
            if (room is null || room.SchoolId != request.SchoolId)
                return Result.Failure<Guid>("The selected room was not found in this school.");
            if (request.CampusId.HasValue && room.CampusId != request.CampusId)
                return Result.Failure<Guid>("The selected room must belong to the classroom campus.");
        }

        var exists = await _classRooms.ExistsAsync(request.SchoolId, request.GradeLevelId, request.AcademicYearId, request.Name, ct);
        if (exists)
            return Result.Failure<Guid>("A classroom with this name already exists for the same grade level and academic year.");

        var classRoom = ClassRoom.Create(request.SchoolId, request.GradeLevelId, request.AcademicYearId, request.Name, request.RoomId, request.CampusId);
        await _classRooms.AddAsync(classRoom, ct);

        return Result.Success(classRoom.Id);
    }
}

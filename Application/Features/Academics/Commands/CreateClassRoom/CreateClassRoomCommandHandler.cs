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

    public CreateClassRoomCommandHandler(
        IClassRoomRepository classRooms,
        IGradeLevelRepository gradeLevels,
        IAcademicYearRepository academicYears,
        IRoomRepository rooms)
    {
        _classRooms = classRooms;
        _gradeLevels = gradeLevels;
        _academicYears = academicYears;
        _rooms = rooms;
    }

    public async Task<Result<Guid>> Handle(CreateClassRoomCommand request, CancellationToken ct)
    {
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
        }

        var exists = await _classRooms.ExistsAsync(request.SchoolId, request.GradeLevelId, request.AcademicYearId, request.Name, ct);
        if (exists)
            return Result.Failure<Guid>("A classroom with this name already exists for the same grade level and academic year.");

        var classRoom = ClassRoom.Create(request.SchoolId, request.GradeLevelId, request.AcademicYearId, request.Name, request.RoomId);
        await _classRooms.AddAsync(classRoom, ct);

        return Result.Success(classRoom.Id);
    }
}

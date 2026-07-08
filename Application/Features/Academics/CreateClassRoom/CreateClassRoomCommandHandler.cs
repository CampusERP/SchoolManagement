using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.CreateClassRoom;

public class CreateClassRoomCommandHandler : IRequestHandler<CreateClassRoomCommand, Result<Guid>>
{
    private readonly IClassRoomRepository _classRooms;
    private readonly IGradeLevelRepository _gradeLevels;
    private readonly IAcademicYearRepository _academicYears;
    private readonly ITenantContext _tenant;

    public CreateClassRoomCommandHandler(
        IClassRoomRepository classRooms,
        IGradeLevelRepository gradeLevels,
        IAcademicYearRepository academicYears,
        ITenantContext tenant)
    {
        _classRooms = classRooms;
        _gradeLevels = gradeLevels;
        _academicYears = academicYears;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateClassRoomCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var gradeLevel = await _gradeLevels.GetByIdAsync(request.GradeLevelId, ct);
        if (gradeLevel is null)
            return Result.Failure<Guid>($"GradeLevel with ID '{request.GradeLevelId}' was not found.");

        var academicYear = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (academicYear is null)
            return Result.Failure<Guid>($"AcademicYear with ID '{request.AcademicYearId}' was not found.");

        var exists = await _classRooms.ExistsAsync(schoolId, request.GradeLevelId, request.AcademicYearId, request.Name, ct);
        if (exists)
            return Result.Failure<Guid>("A classroom with this name already exists for the same grade level and academic year.");

        var classRoom = ClassRoom.Create(schoolId, request.GradeLevelId, request.AcademicYearId, request.Name);
        await _classRooms.AddAsync(classRoom, ct);

        return Result.Success(classRoom.Id);
    }
}

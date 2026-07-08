using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Enrollment;

namespace Application.Features.Enrollment.Commands.EnrollStudent;

public class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, Result<Guid>>
{
    private readonly IStudentEnrollmentRepository _enrollments;
    private readonly IStudentRepository _students;
    private readonly IClassRoomRepository _classRooms;
    private readonly IAcademicYearRepository _academicYears;
    private readonly ITenantContext _tenant;

    public EnrollStudentCommandHandler(
        IStudentEnrollmentRepository enrollments,
        IStudentRepository students,
        IClassRoomRepository classRooms,
        IAcademicYearRepository academicYears,
        ITenantContext tenant)
    {
        _enrollments = enrollments;
        _students = students;
        _classRooms = classRooms;
        _academicYears = academicYears;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(EnrollStudentCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result.Failure<Guid>($"Student with ID '{request.StudentId}' was not found.");

        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null)
            return Result.Failure<Guid>($"ClassRoom with ID '{request.ClassRoomId}' was not found.");

        var academicYear = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (academicYear is null)
            return Result.Failure<Guid>($"AcademicYear with ID '{request.AcademicYearId}' was not found.");

        var alreadyEnrolled = await _enrollments.ExistsAsync(schoolId, request.StudentId, request.AcademicYearId, ct);
        if (alreadyEnrolled)
            return Result.Failure<Guid>("Student is already enrolled in this academic year.");

        var enrollment = StudentEnrollment.Create(schoolId, request.StudentId, request.ClassRoomId, request.AcademicYearId);
        await _enrollments.AddAsync(enrollment, ct);

        return Result.Success(enrollment.Id);
    }
}

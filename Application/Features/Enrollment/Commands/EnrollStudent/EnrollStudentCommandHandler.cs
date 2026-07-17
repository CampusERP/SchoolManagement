using MediatR;
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

    public EnrollStudentCommandHandler(
        IStudentEnrollmentRepository enrollments,
        IStudentRepository students,
        IClassRoomRepository classRooms,
        IAcademicYearRepository academicYears)
    {
        _enrollments = enrollments;
        _students = students;
        _classRooms = classRooms;
        _academicYears = academicYears;
    }

    public async Task<Result<Guid>> Handle(EnrollStudentCommand request, CancellationToken ct)
    {
        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result.Failure<Guid>($"Student with ID '{request.StudentId}' was not found.");

        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null)
            return Result.Failure<Guid>($"ClassRoom with ID '{request.ClassRoomId}' was not found.");

        var academicYear = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (academicYear is null)
            return Result.Failure<Guid>($"AcademicYear with ID '{request.AcademicYearId}' was not found.");

        if (academicYear.Status == Domain.Enums.AcademicYearStatus.Closed ||
            academicYear.Status == Domain.Enums.AcademicYearStatus.Archived)
            return Result.Failure<Guid>("Cannot enroll a student into a closed or archived academic year.");

        var alreadyEnrolled = await _enrollments.ExistsAsync(request.SchoolId, request.StudentId, request.AcademicYearId, ct);
        if (alreadyEnrolled)
            return Result.Failure<Guid>("Student is already enrolled in this academic year.");

        var enrollment = StudentEnrollment.Create(request.SchoolId, request.StudentId, request.ClassRoomId, request.AcademicYearId);
        
        await _enrollments.AddAsync(enrollment, ct);
        return Result.Success(enrollment.Id);
    }
}
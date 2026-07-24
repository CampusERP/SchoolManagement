using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Enrollment;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Enrollment.Commands.EnrollTeacher;

public class EnrollTeacherCommandHandler : IRequestHandler<EnrollTeacherCommand, Result<Guid>>
{
    private readonly ITeacherEnrollmentRepository _enrollments;
    private readonly ITeacherRepository _teachers;
    private readonly IClassRoomRepository _classRooms;

    public EnrollTeacherCommandHandler(
        ITeacherEnrollmentRepository enrollments,
        ITeacherRepository teachers,
        IClassRoomRepository classRooms)
    {
        _enrollments = enrollments;
        _teachers = teachers;
        _classRooms = classRooms;
    }

    public async Task<Result<Guid>> Handle(EnrollTeacherCommand request, CancellationToken ct)
    {
        var teacher = await _teachers.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null || teacher.SchoolId != request.SchoolId)
            return Result.Failure<Guid>($"Teacher with ID '{request.TeacherId}' was not found in this school.");

        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null || classRoom.SchoolId != request.SchoolId)
            return Result.Failure<Guid>($"Classroom with ID '{request.ClassRoomId}' was not found in this school.");

        var alreadyEnrolled = await _enrollments.ExistsAsync(request.SchoolId, request.TeacherId, request.TermId, ct);
        if (alreadyEnrolled)
            return Result.Failure<Guid>("Teacher is already enrolled for this term.");

        var enrollment = TeacherEnrollment.Create(request.SchoolId, request.TeacherId, request.ClassRoomId, request.TermId);

        await _enrollments.AddAsync(enrollment, ct);
        return Result.Success(enrollment.Id);
    }
}

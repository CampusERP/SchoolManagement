using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Enrollment;

namespace Application.Features.Enrollment.Commands.AssignTeacher;

public class AssignTeacherCommandHandler : IRequestHandler<AssignTeacherCommand, Result<Guid>>
{
    private readonly ITeachingAssignmentRepository _assignments;
    private readonly ITeacherRepository _teachers;
    private readonly IClassRoomRepository _classRooms;
    private readonly ITenantContext _tenant;

    public AssignTeacherCommandHandler(
        ITeachingAssignmentRepository assignments,
        ITeacherRepository teachers,
        IClassRoomRepository classRooms,
        ITenantContext tenant)
    {
        _assignments = assignments;
        _teachers = teachers;
        _classRooms = classRooms;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(AssignTeacherCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var teacher = await _teachers.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null)
            return Result.Failure<Guid>($"Teacher with ID '{request.TeacherId}' was not found.");

        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null)
            return Result.Failure<Guid>($"ClassRoom with ID '{request.ClassRoomId}' was not found.");

        var duplicate = await _assignments.ExistsAsync(schoolId, request.TeacherId, request.ClassRoomId, request.SubjectId, request.TermId, ct);
        if (duplicate)
            return Result.Failure<Guid>("This teacher is already assigned to this class for the same subject and term.");

        var assignment = TeachingAssignment.Create(schoolId, request.TeacherId, request.SubjectId, request.ClassRoomId, request.TermId);
        await _assignments.AddAsync(assignment, ct);

        return Result.Success(assignment.Id);
    }
}

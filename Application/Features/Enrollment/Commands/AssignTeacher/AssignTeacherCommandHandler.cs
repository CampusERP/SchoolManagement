using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Enrollment;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Enrollment.Commands.AssignTeacher;

public class AssignTeacherCommandHandler : IRequestHandler<AssignTeacherCommand, Result<Guid>>
{
    private readonly ITeachingAssignmentRepository _assignments;
    private readonly ITeacherRepository _teachers;
    private readonly IClassRoomRepository _classRooms;
    private readonly ICurriculumSubjectRepository _curriculumSubjects;

    public AssignTeacherCommandHandler(
        ITeachingAssignmentRepository assignments,
        ITeacherRepository teachers,
        IClassRoomRepository classRooms,
        ICurriculumSubjectRepository curriculumSubjects)
    {
        _assignments = assignments;
        _teachers = teachers;
        _classRooms = classRooms;
        _curriculumSubjects = curriculumSubjects;
    }

    public async Task<Result<Guid>> Handle(AssignTeacherCommand request, CancellationToken ct)
    {
        var teacher = await _teachers.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null || teacher.SchoolId != request.SchoolId)
            return Result.Failure<Guid>($"Teacher with ID '{request.TeacherId}' was not found in this school.");

        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null || classRoom.SchoolId != request.SchoolId)
            return Result.Failure<Guid>($"ClassRoom with ID '{request.ClassRoomId}' was not found in this school.");

        if (!await _curriculumSubjects.ExistsAsync(classRoom.GradeLevelId, request.SubjectId, ct))
            return Result.Failure<Guid>("The selected subject is not assigned to this classroom's grade level.");

        var duplicate = await _assignments.ExistsAsync(request.SchoolId, request.TeacherId, request.ClassRoomId, request.SubjectId, request.TermId, ct);
        if (duplicate)
            return Result.Failure<Guid>("This teacher is already assigned to this class for the same subject and term.");

        var existingAssignments = await _assignments.GetByTeacherAndTermAsync(
            request.SchoolId, request.TeacherId, request.TermId, ct);

        var allExistingSlots = existingAssignments
            .SelectMany(a => a.Schedules)
            .ToList();

        var assignment = TeachingAssignment.Create(request.SchoolId, request.TeacherId, request.SubjectId, request.ClassRoomId, request.TermId);

        foreach (var slot in request.ScheduleSlots)
        {
            var clash = allExistingSlots.Any(s =>
                s.DayOfWeek == slot.DayOfWeek &&
                slot.StartTime < s.EndTime &&
                slot.EndTime > s.StartTime);
             
            if(clash)
                return Result.Failure<Guid>($"Teacher is already scheduled on {slot.DayOfWeek} between {slot.StartTime} and {slot.EndTime}.");

            try
            {
                assignment.AddSchedule(slot.RoomId, slot.DayOfWeek, slot.StartTime, slot.EndTime);
            }
            catch (DomainException ex)
            {
                return Result.Failure<Guid>(ex.Message);
            }
        }

        await _assignments.AddAsync(assignment, ct);
        return Result.Success(assignment.Id);
    }
}

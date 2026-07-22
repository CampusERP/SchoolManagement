using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Enrollment;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Enrollment.Commands.UpdateTeachingAssignment;

public class UpdateTeachingAssignmentCommandHandler : IRequestHandler<UpdateTeachingAssignmentCommand, Result<Guid>>
{
    private readonly ITeachingAssignmentRepository _assignments;
    private readonly ITeacherRepository _teachers;
    private readonly IClassRoomRepository _classRooms;

    public UpdateTeachingAssignmentCommandHandler(
        ITeachingAssignmentRepository assignments,
        ITeacherRepository teachers,
        IClassRoomRepository classRooms)
    {
        _assignments = assignments;
        _teachers = teachers;
        _classRooms = classRooms;
    }

    public async Task<Result<Guid>> Handle(UpdateTeachingAssignmentCommand request, CancellationToken ct)
    {
        var assignment = await _assignments.GetByIdAsync(request.TeachingAssignmentId, ct);
        if (assignment is null)
            return Result.Failure<Guid>("Teaching assignment not found.");

        if (assignment.TeacherId != request.TeacherId)
            return Result.Failure<Guid>("This assignment does not belong to the specified teacher.");

        var teacher = await _teachers.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null || teacher.SchoolId != request.SchoolId)
            return Result.Failure<Guid>($"Teacher with ID '{request.TeacherId}' was not found in this school.");

        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null || classRoom.SchoolId != request.SchoolId)
            return Result.Failure<Guid>($"ClassRoom with ID '{request.ClassRoomId}' was not found in this school.");

        var duplicate = await _assignments.ExistsAsync(request.SchoolId, request.TeacherId, request.ClassRoomId, request.SubjectId, request.TermId, ct);
        if (duplicate && (request.ClassRoomId != assignment.ClassRoomId || request.SubjectId != assignment.SubjectId || request.TermId != assignment.TermId))
            return Result.Failure<Guid>("This teacher is already assigned to this class for the same subject and term.");

        var otherAssignments = await _assignments.GetByTeacherAndTermAsync(
            request.SchoolId, request.TeacherId, request.TermId, ct);

        var allOtherSlots = otherAssignments
            .Where(a => a.Id != request.TeachingAssignmentId)
            .SelectMany(a => a.Schedules)
            .ToList();

        assignment.UpdateSubject(request.SubjectId);
        assignment.UpdateClassRoom(request.ClassRoomId);
        assignment.UpdateTerm(request.TermId);
        assignment.ClearSchedules();

        foreach (var slot in request.ScheduleSlots)
        {
            var clash = allOtherSlots.Any(s =>
                s.DayOfWeek == slot.DayOfWeek &&
                slot.StartTime < s.EndTime &&
                slot.EndTime > s.StartTime);

            if (clash)
                return Result.Failure<Guid>($"Teacher is already scheduled on day {slot.DayOfWeek} between {slot.StartTime} and {slot.EndTime}.");

            try
            {
                assignment.AddSchedule(slot.RoomId, slot.DayOfWeek, slot.StartTime, slot.EndTime);
            }
            catch (DomainException ex)
            {
                return Result.Failure<Guid>(ex.Message);
            }
        }

        await _assignments.UpdateAsync(assignment, ct);
        return Result.Success(assignment.Id);
    }
}

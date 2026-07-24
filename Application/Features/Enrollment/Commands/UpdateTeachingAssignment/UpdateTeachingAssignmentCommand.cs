using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Features.Enrollment.Commands.AssignTeacher;
using Domain.Entities.Enrollment;
using MediatR;

namespace Application.Features.Enrollment.Commands.UpdateTeachingAssignment;

public record UpdateTeachingAssignmentCommand(
    Guid SchoolId,
    Guid TeachingAssignmentId,
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassRoomId,
    Guid TermId,
    List<ScheduleSlot> ScheduleSlots) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;

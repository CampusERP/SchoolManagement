namespace Application.Features.People.Queries.GetTeacherById;

public record TeachingAssignmentDetailDto(
    Guid Id,
    Guid SubjectId,
    string SubjectName,
    Guid ClassRoomId,
    string ClassRoomName,
    Guid TermId,
    string TermName,
    List<TeachingAssignmentScheduleSlotDto> Schedules);

namespace Application.Features.Academics.Queries.GetClassRooms;

public record ClassRoomTeachingAssignmentDto(
    Guid Id,
    string SubjectName,
    string SubjectCode,
    string TeacherFirstName,
    string TeacherLastName,
    string TermName);

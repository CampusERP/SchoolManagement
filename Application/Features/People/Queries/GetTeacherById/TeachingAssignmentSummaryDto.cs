namespace Application.Features.People.Queries.GetTeacherById;

public record TeachingAssignmentSummaryDto(Guid Id, string SubjectName,
    string ClassRoomName, string TermName);

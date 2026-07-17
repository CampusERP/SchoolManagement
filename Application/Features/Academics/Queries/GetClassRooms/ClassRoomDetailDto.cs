namespace Application.Features.Academics.Queries.GetClassRooms;

public record ClassRoomDetailDto(Guid Id, string Name, string GradeLevel,
    string AcademicYear, int EnrolledStudents, int TeachingAssignmentCount);

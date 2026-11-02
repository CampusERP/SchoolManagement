namespace Application.Features.Academics.Queries.GetClassRooms;

public record ClassRoomDetailDto(Guid Id, string Name, Guid GradeLevelId, string GradeLevel,
    string AcademicYear, int EnrolledStudents, int TeachingAssignmentCount,
    Guid? RoomId, string? RoomName);

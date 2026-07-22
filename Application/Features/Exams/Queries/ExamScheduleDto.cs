namespace Application.Features.Exams.Queries;

public record ExamScheduleDto(Guid Id, Guid ClassRoomId, string ClassRoomName, Guid RoomId, string RoomName, DateTime ExamDate);

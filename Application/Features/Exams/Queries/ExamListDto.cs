namespace Application.Features.Exams.Queries;

public record ExamListDto(Guid Id, string Name, string SubjectName,
    string TermName, decimal MaxScore, bool IsLocked, int ScheduleCount);

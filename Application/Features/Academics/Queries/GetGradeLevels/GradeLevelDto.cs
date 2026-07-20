namespace Application.Features.Academics.Queries.GetGradeLevels;

public record GradeLevelDto(Guid Id, string Name, int Sequence,
    Guid EducationStageId, string EducationStage, int ClassRoomCount);

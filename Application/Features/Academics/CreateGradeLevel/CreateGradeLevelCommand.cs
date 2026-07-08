using Application.Common.Behaviors;

namespace Application.Features.Academics.CreateGradeLevel;

public record CreateGradeLevelCommand(
    Guid EducationStageId,
    string Name,
    int Sequence) : ICommand<Guid>, IBaseCommand;

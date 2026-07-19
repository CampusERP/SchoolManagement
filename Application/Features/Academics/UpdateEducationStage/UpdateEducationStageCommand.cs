using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.UpdateEducationStage;

public record UpdateEducationStageCommand(
    Guid EducationStageId,
    string Name) : ICommand, IBaseCommand;

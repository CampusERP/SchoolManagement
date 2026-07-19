using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.DeleteEducationStage;

public record DeleteEducationStageCommand(Guid EducationStageId)
    : ICommand, IBaseCommand;

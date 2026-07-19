using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.CreateEducationStage;

public record CreateEducationStageCommand(string Name)
    : ICommand<Guid>, IBaseCommand;

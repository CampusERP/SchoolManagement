using MediatR;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Academics;

namespace Application.Features.Academics.CreateEducationStage;

public class CreateEducationStageCommandHandler
    : IRequestHandler<CreateEducationStageCommand, Result<Guid>>
{
    private readonly IEducationStageRepository _educationStages;

    public CreateEducationStageCommandHandler(IEducationStageRepository educationStages)
    {
        _educationStages = educationStages;
    }

    public async Task<Result<Guid>> Handle(
        CreateEducationStageCommand request, CancellationToken ct)
    {
        if (await _educationStages.ExistsByNameAsync(request.Name, ct))
            return Result.Failure<Guid>($"An education stage named '{request.Name}' already exists.");

        var stage = EducationStage.Create(request.Name);
        await _educationStages.AddAsync(stage, ct);

        return Result.Success(stage.Id);
    }
}

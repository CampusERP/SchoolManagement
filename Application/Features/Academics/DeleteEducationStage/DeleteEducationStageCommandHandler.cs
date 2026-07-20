using MediatR;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;

namespace Application.Features.Academics.DeleteEducationStage;

public class DeleteEducationStageCommandHandler : IRequestHandler<DeleteEducationStageCommand, Result>
{
    private readonly IEducationStageRepository _educationStages;

    public DeleteEducationStageCommandHandler(IEducationStageRepository educationStages)
    {
        _educationStages = educationStages;
    }

    public async Task<Result> Handle(DeleteEducationStageCommand request, CancellationToken ct)
    {
        var stage = await _educationStages.GetByIdAsync(request.EducationStageId, ct);
        if (stage is null) return Result.Failure("Education stage not found.");

        await _educationStages.RemoveAsync(stage, ct);

        return Result.Success();
    }
}

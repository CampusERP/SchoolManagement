using MediatR;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;

namespace Application.Features.Academics.UpdateEducationStage;

public class UpdateEducationStageCommandHandler : IRequestHandler<UpdateEducationStageCommand, Result>
{
    private readonly IEducationStageRepository _educationStages;

    public UpdateEducationStageCommandHandler(IEducationStageRepository educationStages)
    {
        _educationStages = educationStages;
    }

    public async Task<Result> Handle(UpdateEducationStageCommand request, CancellationToken ct)
    {
        var stage = await _educationStages.GetByIdAsync(request.EducationStageId, ct);
        if (stage is null) return Result.Failure("Education stage not found.");

        try
        {
            stage.Update(request.Name);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}

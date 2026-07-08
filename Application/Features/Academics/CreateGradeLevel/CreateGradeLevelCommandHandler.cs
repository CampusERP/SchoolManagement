using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.CreateGradeLevel;

public class CreateGradeLevelCommandHandler : IRequestHandler<CreateGradeLevelCommand, Result<Guid>>
{
    private readonly IGradeLevelRepository _gradeLevels;
    private readonly ITenantContext _tenant;

    public CreateGradeLevelCommandHandler(IGradeLevelRepository gradeLevels, ITenantContext tenant)
    {
        _gradeLevels = gradeLevels;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateGradeLevelCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var gradeLevel = GradeLevel.Create(schoolId, request.EducationStageId, request.Name, request.Sequence);
        await _gradeLevels.AddAsync(gradeLevel, ct);

        return Result.Success(gradeLevel.Id);
    }
}

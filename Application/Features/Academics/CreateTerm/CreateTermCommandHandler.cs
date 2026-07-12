using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;

namespace Application.Features.Academics.CreateTerm;

public class CreateTermCommandHandler : IRequestHandler<CreateTermCommand, Result<Guid>>
{
    private readonly IAcademicYearRepository _academicYears;
    private readonly ITenantContext _tenant;

    public CreateTermCommandHandler(IAcademicYearRepository academicYears, ITenantContext tenant)
    {
        _academicYears = academicYears;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateTermCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var academicYear = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (academicYear is null)
            return Result.Failure<Guid>($"AcademicYear with ID '{request.AcademicYearId}' was not found.");

        var term = academicYear.AddTerm(request.Name, request.Sequence, request.StartDate, request.EndDate);

        return Result.Success(term.Id);
    }
}

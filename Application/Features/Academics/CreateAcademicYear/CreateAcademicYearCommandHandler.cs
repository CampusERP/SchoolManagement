using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.CreateAcademicYear;

public class CreateAcademicYearCommandHandler : IRequestHandler<CreateAcademicYearCommand, Result<Guid>>
{
    private readonly IAcademicYearRepository _academicYears;
    private readonly ITenantContext _tenant;

    public CreateAcademicYearCommandHandler(IAcademicYearRepository academicYears, ITenantContext tenant)
    {
        _academicYears = academicYears;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateAcademicYearCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var academicYear = AcademicYear.Create(schoolId, request.Name, request.StartDate, request.EndDate);
        await _academicYears.AddAsync(academicYear, ct);

        return Result.Success(academicYear.Id);
    }
}

using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.CreateAcademicYear;

public class CreateAcademicYearCommandHandler : IRequestHandler<CreateAcademicYearCommand, Result<Guid>>
{
    private readonly IAcademicYearRepository _academicYears;

    public CreateAcademicYearCommandHandler(IAcademicYearRepository academicYears)
    {
        _academicYears = academicYears;
    }

    public async Task<Result<Guid>> Handle(CreateAcademicYearCommand request, CancellationToken ct)
    {
        if (request.SetAsCurrent)
        {
            var alreadyCurrent = await _academicYears.HasCurrentAsync(request.SchoolId, ct);
            if (alreadyCurrent)
                return Result.Failure<Guid>("There is already a current academic year for this school. Close it first before setting a new one as current.");
        }

        var academicYear = AcademicYear.Create(request.SchoolId, request.Name, request.StartDate, request.EndDate);

        if (request.SetAsCurrent)
            academicYear.Activate();

        await _academicYears.AddAsync(academicYear, ct);

        return Result.Success(academicYear.Id);
    }
}
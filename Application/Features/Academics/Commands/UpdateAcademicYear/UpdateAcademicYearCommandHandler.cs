using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.UpdateAcademicYear;

public class UpdateAcademicYearCommandHandler : IRequestHandler<UpdateAcademicYearCommand, Result>
{
    private readonly IAcademicYearRepository _academicYears;

    public UpdateAcademicYearCommandHandler(IAcademicYearRepository academicYears)
    {
        _academicYears = academicYears;
    }

    public async Task<Result> Handle(UpdateAcademicYearCommand request, CancellationToken ct)
    {
        var year = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (year is null) return Result.Failure("Academic year not found.");

        try
        {
            year.Update(request.Name, request.StartDate, request.EndDate);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}

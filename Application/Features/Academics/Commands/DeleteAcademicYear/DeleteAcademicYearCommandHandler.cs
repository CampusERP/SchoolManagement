using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.DeleteAcademicYear;

public sealed class DeleteAcademicYearCommandHandler : IRequestHandler<DeleteAcademicYearCommand, Result>
{
    private readonly IAcademicYearRepository _academicYears;

    public DeleteAcademicYearCommandHandler(IAcademicYearRepository academicYears) => _academicYears = academicYears;

    public async Task<Result> Handle(DeleteAcademicYearCommand request, CancellationToken ct)
    {
        var year = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (year is null || year.SchoolId != request.SchoolId)
            return Result.Failure("Academic year not found.");

        if (year.IsCurrent)
            return Result.Failure("The current academic year cannot be deleted. Close or replace it first.");

        await _academicYears.DeleteAsync(year, ct);
        return Result.Success();
    }
}

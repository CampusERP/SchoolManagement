using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.DeleteTerm;

public class DeleteTermCommandHandler : IRequestHandler<DeleteTermCommand, Result>
{
    private readonly IAcademicYearRepository _academicYears;

    public DeleteTermCommandHandler(IAcademicYearRepository academicYears)
    {
        _academicYears = academicYears;
    }

    public async Task<Result> Handle(DeleteTermCommand request, CancellationToken ct)
    {
        var year = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (year is null)
            return Result.Failure("Academic year not found.");

        if (year.SchoolId != request.SchoolId)
            return Result.Failure("Academic year does not belong to this school.");

        var term = year.Terms.FirstOrDefault(t => t.Id == request.TermId);
        if (term is null)
            return Result.Failure("Term not found.");

        try
        {
            await _academicYears.DeleteTermAsync(year, term, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}

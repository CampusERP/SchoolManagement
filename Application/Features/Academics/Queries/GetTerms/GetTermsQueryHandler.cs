using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetTerms;

public class GetTermsQueryHandler : IRequestHandler<GetTermsQuery, Result<List<TermDto>>>
{
    private readonly IAcademicYearRepository _academicYears;

    public GetTermsQueryHandler(IAcademicYearRepository academicYears)
    {
        _academicYears = academicYears;
    }

    public async Task<Result<List<TermDto>>> Handle(GetTermsQuery request, CancellationToken ct)
    {
        var year = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (year is null)
            return Result.Failure<List<TermDto>>("Academic year not found.");

        if (year.SchoolId != request.SchoolId)
            return Result.Failure<List<TermDto>>("Academic year does not belong to this school.");

        var terms = year.Terms
            .OrderBy(t => t.Sequence)
            .Select(t => new TermDto(t.Id, t.Name, t.Sequence, t.StartDate, t.EndDate))
            .ToList();

        return Result.Success(terms);
    }
}

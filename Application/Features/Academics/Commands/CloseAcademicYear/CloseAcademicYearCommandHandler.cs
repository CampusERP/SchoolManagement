using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.CloseAcademicYear;

public class CloseAcademicYearCommandHandler : IRequestHandler<CloseAcademicYearCommand, Result>
{
    private readonly IAcademicYearRepository _years;
    public CloseAcademicYearCommandHandler(IAcademicYearRepository years) => _years = years;

    public async Task<Result> Handle(CloseAcademicYearCommand request, CancellationToken ct)
    {
        var year = await _years.GetByIdAsync(request.AcademicYearId, ct);
        if (year is null) throw new NotFoundException("AcademicYear", request.AcademicYearId);
        try { year.Close(); return Result.Success(); }
        catch (Domain.Exceptions.DomainException ex) { return Result.Failure(ex.Message); }
    }
}
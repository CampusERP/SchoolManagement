using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;

namespace Application.Features.Academics.Commands.CreateTerm;

public class CreateTermCommandHandler : IRequestHandler<CreateTermCommand, Result<Guid>>
{
    private readonly IAcademicYearRepository _academicYears;

    public CreateTermCommandHandler(IAcademicYearRepository academicYears)
    {
        _academicYears = academicYears;
    }

    public async Task<Result<Guid>> Handle(CreateTermCommand request, CancellationToken ct)
    {

        var academicYear = await _academicYears.GetByIdAsync(request.AcademicYearId, ct);
        if (academicYear is null)
            return Result.Failure<Guid>($"AcademicYear with ID '{request.AcademicYearId}' was not found.");

        try
        {
            var term = academicYear.AddTerm(request.Name, request.Sequence, request.StartDate, request.EndDate);
            return Result.Success(term.Id);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }
    }
}
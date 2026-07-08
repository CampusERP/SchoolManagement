using Application.Common.Behaviors;

namespace Application.Features.Academics.CreateTerm;

public record CreateTermCommand(
    Guid AcademicYearId,
    string Name,
    int Sequence,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Guid>, IBaseCommand;

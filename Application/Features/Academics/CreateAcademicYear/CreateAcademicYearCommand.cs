using Application.Common.Behaviors;

namespace Application.Features.Academics.CreateAcademicYear;

public record CreateAcademicYearCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Guid>, IBaseCommand;

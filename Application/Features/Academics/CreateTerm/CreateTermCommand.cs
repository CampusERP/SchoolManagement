using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.CreateTerm;

public record CreateTermCommand(
    Guid SchoolId,
    Guid AcademicYearId,
    string Name,
    int Sequence,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;

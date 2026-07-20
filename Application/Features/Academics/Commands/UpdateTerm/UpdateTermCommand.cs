using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.UpdateTerm;

public record UpdateTermCommand(
    Guid SchoolId,
    Guid AcademicYearId,
    Guid TermId,
    string Name,
    int Sequence,
    DateTime StartDate,
    DateTime EndDate) : ICommand, IBaseCommand, ITenantScopedRequest;

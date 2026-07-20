using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.UpdateAcademicYear;

public record UpdateAcademicYearCommand(
    Guid SchoolId,
    Guid AcademicYearId,
    string Name,
    DateTime StartDate,
    DateTime EndDate) : ICommand, IBaseCommand, ITenantScopedRequest;

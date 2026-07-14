using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.CreateAcademicYear;

public record CreateAcademicYearCommand(
    Guid SchoolId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool SetAsCurrent = false) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
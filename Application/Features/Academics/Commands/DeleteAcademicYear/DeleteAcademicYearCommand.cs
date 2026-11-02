using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.DeleteAcademicYear;

public record DeleteAcademicYearCommand(Guid SchoolId, Guid AcademicYearId)
    : ICommand, IBaseCommand, ITenantScopedRequest;

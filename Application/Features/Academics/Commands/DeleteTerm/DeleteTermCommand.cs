using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Academics.Commands.DeleteTerm;

public record DeleteTermCommand(
    Guid SchoolId,
    Guid AcademicYearId,
    Guid TermId) : ICommand, IBaseCommand, ITenantScopedRequest;

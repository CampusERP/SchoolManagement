using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Exams.Commands;

public record GenerateReportCardCommand(Guid SchoolId, Guid StudentEnrollmentId, Guid TermId)
    : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;

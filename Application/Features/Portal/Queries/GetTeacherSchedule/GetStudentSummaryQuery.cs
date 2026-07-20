using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public record GetStudentSummaryQuery(Guid SchoolId, Guid StudentEnrollmentId)
    : IRequest<Result<StudentSummaryDto>>, ITenantScopedRequest;

using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetStudentDashboard;

public record GetStudentDashboardQuery(Guid SchoolId, Guid StudentEnrollmentId)
    : IRequest<Result<StudentDashboardDto>>, ITenantScopedRequest;

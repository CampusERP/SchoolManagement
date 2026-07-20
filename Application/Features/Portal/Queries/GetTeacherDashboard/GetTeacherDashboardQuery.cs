using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherDashboard;

public record GetTeacherDashboardQuery(Guid SchoolId)
    : IRequest<Result<TeacherDashboardDto>>, ITenantScopedRequest;

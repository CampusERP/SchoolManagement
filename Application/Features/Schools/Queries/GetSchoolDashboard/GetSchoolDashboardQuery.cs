using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Queries.GetSchoolDashboard;

public record GetSchoolDashboardQuery(Guid SchoolId)
    : IRequest<Result<SchoolDashboardDto>>, ITenantScopedRequest;

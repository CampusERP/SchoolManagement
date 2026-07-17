using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetGradeLevels;

public record GetGradeLevelsQuery(Guid SchoolId)
    : IRequest<Result<List<GradeLevelDto>>>, ITenantScopedRequest;

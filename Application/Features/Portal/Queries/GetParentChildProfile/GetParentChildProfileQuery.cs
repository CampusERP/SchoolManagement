using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildProfile;

public record GetParentChildProfileQuery(Guid SchoolId, Guid StudentId)
    : IRequest<Result<ParentChildProfileDto>>, ITenantScopedRequest;

using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.People.Queries.GetParents;
using MediatR;

namespace Application.Features.People.Queries.GetParentById;

public record GetParentByIdQuery(Guid SchoolId, Guid ParentId)
    : IRequest<Result<ParentDetailDto>>, ITenantScopedRequest;

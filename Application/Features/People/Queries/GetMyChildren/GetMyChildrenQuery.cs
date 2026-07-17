using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetMyChildren;

public record GetMyChildrenQuery(Guid SchoolId)
    : IRequest<Result<List<ChildSummaryDto>>>, ITenantScopedRequest;

using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.People.Queries.GetParents;
using MediatR;

namespace Application.Features.People.Queries.GetParentById;

public class GetParentByIdQueryHandler : IRequestHandler<GetParentByIdQuery, Result<ParentDetailDto>>
{
    private readonly IParentReadService _parents;

    public GetParentByIdQueryHandler(IParentReadService parents) => _parents = parents;

    public async Task<Result<ParentDetailDto>> Handle(GetParentByIdQuery request, CancellationToken ct)
    {
        var parent = await _parents.GetParentByIdAsync(request.SchoolId, request.ParentId, ct);
        return parent is null
            ? Result.Failure<ParentDetailDto>("Parent not found.")
            : Result.Success(parent);
    }
}

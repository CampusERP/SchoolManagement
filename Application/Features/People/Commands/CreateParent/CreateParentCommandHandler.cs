using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;

namespace Application.Features.People.Commands.CreateParent;

public class CreateParentCommandHandler : IRequestHandler<CreateParentCommand, Result<Guid>>
{
    private readonly IParentRepository _parents;
    private readonly ITenantContext _tenant;

    public CreateParentCommandHandler(IParentRepository parents, ITenantContext tenant)
    {
        _parents = parents;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateParentCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var parent = Parent.Create(schoolId, request.ApplicationUserId, request.FirstName, request.LastName);
        await _parents.AddAsync(parent, ct);

        return Result.Success(parent.Id);
    }
}

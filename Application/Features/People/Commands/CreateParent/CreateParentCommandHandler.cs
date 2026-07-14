using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.CreateParent;

public class CreateParentCommandHandler : IRequestHandler<CreateParentCommand, Result<Guid>>
{
    private readonly IParentRepository _parents;
    private readonly IIdentityService _identityService;

    public CreateParentCommandHandler(IParentRepository parents, IIdentityService identityService)
    {
        _parents = parents;
        _identityService = identityService;
    }

    public async Task<Result<Guid>> Handle(CreateParentCommand request, CancellationToken ct)
    {
        var applicationUser = await _identityService.CreateUserAsync(request.Email, request.Password, ct);
        if (applicationUser == null)
            return Result.Failure<Guid>("Failed to create application user.");

        var userId = applicationUser.Value;

        try
        {
            var roleResult = await _identityService.AddToRoleAsync(userId, "Parent", ct);

            if (!roleResult.IsSuccess)
                return Result.Failure<Guid>(roleResult.Error!);

            var parent = Parent.Create(request.SchoolId, userId, request.FirstName, request.LastName);
            await _parents.AddAsync(parent, ct);
            return Result.Success(parent.Id);
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId, ct);
            throw;
        }
    }
}
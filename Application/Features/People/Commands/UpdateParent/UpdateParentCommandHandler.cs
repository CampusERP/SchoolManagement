using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.UpdateParent;

public class UpdateParentCommandHandler : IRequestHandler<UpdateParentCommand, Result>
{
    private readonly IParentRepository _parents;

    public UpdateParentCommandHandler(IParentRepository parents) => _parents = parents;

    public async Task<Result> Handle(UpdateParentCommand request, CancellationToken ct)
    {
        var parent = await _parents.GetByIdAsync(request.ParentId, ct);
        if (parent is null || parent.SchoolId != request.SchoolId)
            return Result.Failure("Parent not found.");

        try
        {
            parent.Update(request.FirstName, request.LastName);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}

using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Commands.DeleteSchoolRegistrationRequest;

public class DeleteSchoolRegistrationRequestCommandHandler
    : IRequestHandler<DeleteSchoolRegistrationRequestCommand, Result>
{
    private readonly ISchoolRegistrationRequestRepository _requests;

    public DeleteSchoolRegistrationRequestCommandHandler(
        ISchoolRegistrationRequestRepository requests)
    {
        _requests = requests;
    }

    public async Task<Result> Handle(
        DeleteSchoolRegistrationRequestCommand request, CancellationToken ct)
    {
        var entity = await _requests.GetByIdAsync(request.RequestId, ct);
        if (entity is null)
            return Result.Failure("Registration request not found.");

        _requests.Remove(entity);
        return Result.Success();
    }
}

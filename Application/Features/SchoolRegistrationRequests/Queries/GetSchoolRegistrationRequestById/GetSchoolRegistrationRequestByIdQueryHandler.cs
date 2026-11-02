using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Queries.GetSchoolRegistrationRequestById;

public class GetSchoolRegistrationRequestByIdQueryHandler
    : IRequestHandler<GetSchoolRegistrationRequestByIdQuery, Result<SchoolRegistrationRequestDto>>
{
    private readonly ISchoolRegistrationRequestRepository _requests;

    public GetSchoolRegistrationRequestByIdQueryHandler(ISchoolRegistrationRequestRepository requests)
    {
        _requests = requests;
    }

    public async Task<Result<SchoolRegistrationRequestDto>> Handle(
        GetSchoolRegistrationRequestByIdQuery request, CancellationToken ct)
    {
        var entity = await _requests.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure<SchoolRegistrationRequestDto>("Registration request not found.");

        return Result.Success(SchoolRegistrationRequestDto.FromEntity(entity));
    }
}

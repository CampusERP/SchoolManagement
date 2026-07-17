using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.StudentDetails;

public record GetMyProfileQuery(Guid SchoolId)
    : IRequest<Result<StudentDetailDto>>, ITenantScopedRequest;

public class GetMyProfileQueryHandler
    : IRequestHandler<GetMyProfileQuery, Result<StudentDetailDto>>
{
    private readonly IStudentReadService _studentReadService;
    private readonly ICurrentUserService _user;

    public GetMyProfileQueryHandler(IStudentReadService studentReadService, ICurrentUserService user)
    {
        _studentReadService = studentReadService;
        _user = user;
    }

    public async Task<Result<StudentDetailDto>> Handle(
        GetMyProfileQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null) return Result.Failure<StudentDetailDto>("Not authenticated.");

        var student = await _studentReadService.GetMyProfileAsync(request.SchoolId, userId.Value, ct);

        if (student is null) return Result.Failure<StudentDetailDto>("Profile not found.");
        return Result.Success(student);
    }
}
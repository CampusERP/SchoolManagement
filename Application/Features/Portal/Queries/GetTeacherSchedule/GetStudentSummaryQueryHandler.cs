using Application.Common.Interfaces.Services;
using MediatR;
using Application.Common.Models;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public class GetStudentSummaryQueryHandler
    : IRequestHandler<GetStudentSummaryQuery, Result<StudentSummaryDto>>
{
    private readonly IPortalReadService _readService;
    public GetStudentSummaryQueryHandler(IPortalReadService readService) => _readService = readService;

    public async Task<Result<StudentSummaryDto>> Handle(
        GetStudentSummaryQuery request, CancellationToken ct)
    {
        var summary = await _readService.GetStudentSummaryAsync(request.StudentEnrollmentId, ct);
        if (summary is null)
            return Result.Failure<StudentSummaryDto>("Enrollment not found.");

        return Result.Success(summary);
    }
}

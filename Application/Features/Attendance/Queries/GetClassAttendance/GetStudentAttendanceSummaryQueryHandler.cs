using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public class GetStudentAttendanceSummaryQueryHandler
    : IRequestHandler<GetStudentAttendanceSummaryQuery, Result<StudentAttendanceSummaryResponse>>
{
    private readonly IAttendanceReadService _readService;

    public GetStudentAttendanceSummaryQueryHandler(IAttendanceReadService readService) => _readService = readService;

    public async Task<Result<StudentAttendanceSummaryResponse>> Handle(
        GetStudentAttendanceSummaryQuery request, CancellationToken ct)
    {
        var summary = await _readService.GetStudentAttendanceSummaryAsync(
            request.StudentEnrollmentId, ct);

        if (summary is null)
            return Result.Failure<StudentAttendanceSummaryResponse>("Enrollment not found.");

        return Result.Success(summary);
    }
}

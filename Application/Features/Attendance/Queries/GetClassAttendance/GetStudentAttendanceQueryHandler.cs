using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public class GetStudentAttendanceQueryHandler
    : IRequestHandler<GetStudentAttendanceQuery, Result<PagedResult<StudentAttendanceSummaryDto>>>
{
    private readonly IAttendanceReadService _readService;

    public GetStudentAttendanceQueryHandler(IAttendanceReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<StudentAttendanceSummaryDto>>> Handle(
        GetStudentAttendanceQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _readService.GetStudentAttendanceAsync(request.StudentEnrollmentId, p, ct);
        return Result.Success(result);
    }
}

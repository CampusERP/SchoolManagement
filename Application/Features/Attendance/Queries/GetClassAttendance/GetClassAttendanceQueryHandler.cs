using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public class GetClassAttendanceQueryHandler
    : IRequestHandler<GetClassAttendanceQuery, Result<ClassAttendanceDto>>
{
    private readonly IAttendanceReadService _readService;

    public GetClassAttendanceQueryHandler(IAttendanceReadService readService) => _readService = readService;

    public async Task<Result<ClassAttendanceDto>> Handle(GetClassAttendanceQuery request, CancellationToken ct)
    {
        var session = await _readService.GetClassAttendanceAsync(request.ClassScheduleId, request.Date, ct);

        if (session is null)
            return Result.Failure<ClassAttendanceDto>("No attendance session found for this date.");

        return Result.Success(session);
    }
}

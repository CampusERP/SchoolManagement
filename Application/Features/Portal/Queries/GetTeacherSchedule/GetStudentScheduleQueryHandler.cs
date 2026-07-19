using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public class GetStudentScheduleQueryHandler
    : IRequestHandler<GetStudentScheduleQuery, Result<List<StudentScheduleSlotDto>>>
{
    private readonly IPortalReadService _readService;
    public GetStudentScheduleQueryHandler(IPortalReadService readService) => _readService = readService;

    public async Task<Result<List<StudentScheduleSlotDto>>> Handle(
        GetStudentScheduleQuery request, CancellationToken ct)
    {
        var slots = await _readService.GetStudentScheduleAsync(request.StudentEnrollmentId, request.TermId, ct);
        return Result.Success(slots);
    }
}

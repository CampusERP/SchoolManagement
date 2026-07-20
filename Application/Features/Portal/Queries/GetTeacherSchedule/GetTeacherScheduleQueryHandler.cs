using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public class GetTeacherScheduleQueryHandler
    : IRequestHandler<GetTeacherScheduleQuery, Result<List<TeacherScheduleSlotDto>>>
{
    private readonly IPortalReadService _readService;
    public GetTeacherScheduleQueryHandler(IPortalReadService readService) => _readService = readService;

    public async Task<Result<List<TeacherScheduleSlotDto>>> Handle(
        GetTeacherScheduleQuery request, CancellationToken ct)
    {
        var slots = await _readService.GetTeacherScheduleAsync(request.TeacherId, request.TermId, ct);
        return Result.Success(slots);
    }
}

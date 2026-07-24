using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public class GetExamSchedulesQueryHandler
    : IRequestHandler<GetExamSchedulesQuery, Result<List<ExamScheduleDto>>>
{
    private readonly IExamReadService _readService;
    public GetExamSchedulesQueryHandler(IExamReadService readService) => _readService = readService;

    public async Task<Result<List<ExamScheduleDto>>> Handle(
        GetExamSchedulesQuery request, CancellationToken ct)
    {
        var items = await _readService.GetExamSchedulesAsync(request.ExamId, ct);
        return Result.Success(items);
    }
}

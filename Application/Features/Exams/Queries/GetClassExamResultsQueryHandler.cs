using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public class GetClassExamResultsQueryHandler
    : IRequestHandler<GetClassExamResultsQuery, Result<List<ClassExamResultDto>>>
{
    private readonly IExamReadService _readService;
    public GetClassExamResultsQueryHandler(IExamReadService readService) => _readService = readService;

    public async Task<Result<List<ClassExamResultDto>>> Handle(
        GetClassExamResultsQuery request, CancellationToken ct)
    {
        var items = await _readService.GetClassExamResultsAsync(request.ExamScheduleId, ct);
        return Result.Success(items);
    }
}

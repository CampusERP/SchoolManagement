using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Exams;
using MediatR;

namespace Application.Features.Exams.Commands;

public class RecordExamResultsCommandHandler : IRequestHandler<RecordExamResultsCommand, Result>
{
    private readonly IExamRepository _exams;
    public RecordExamResultsCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result> Handle(RecordExamResultsCommand request, CancellationToken ct)
    {
        var exam = await _exams.GetByIdAsync(request.ExamId, ct);
        if (exam is null) throw new NotFoundException(nameof(Exam), request.ExamId);

        foreach (var entry in request.Results)
        {
            try
            {
                var result = exam.RecordResult(
                    request.ExamScheduleId, entry.StudentEnrollmentId, entry.Score);
                if (!string.IsNullOrWhiteSpace(entry.Remarks))
                    result.AddRemarks(entry.Remarks);
            }
            catch (Domain.Exceptions.DomainException ex)
            {
                return Result.Failure(
                    $"Error for enrollment {entry.StudentEnrollmentId}: {ex.Message}");
            }
        }

        return Result.Success();
    }
}

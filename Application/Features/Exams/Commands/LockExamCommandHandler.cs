using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Exams;
using MediatR;

namespace Application.Features.Exams.Commands;

public class LockExamCommandHandler : IRequestHandler<LockExamCommand, Result>
{
    private readonly IExamRepository _exams;
    public LockExamCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result> Handle(LockExamCommand request, CancellationToken ct)
    {
        var exam = await _exams.GetByIdAsync(request.ExamId, ct);
        if (exam is null) throw new NotFoundException(nameof(Exam), request.ExamId);
        try { exam.Lock(); return Result.Success(); }
        catch (Domain.Exceptions.DomainException ex) { return Result.Failure(ex.Message); }
    }
}

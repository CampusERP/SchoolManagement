using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Exams;
using MediatR;

namespace Application.Features.Exams.Commands;

public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, Result<Guid>>
{
    private readonly IExamRepository _exams;
    public CreateExamCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<Guid>> Handle(CreateExamCommand request, CancellationToken ct)
    {
        var exam = Exam.Create(request.SchoolId, request.SubjectId,
            request.TermId, request.Name, request.MaxScore);
        await _exams.AddAsync(exam, ct);
        return Result.Success(exam.Id);
    }
}

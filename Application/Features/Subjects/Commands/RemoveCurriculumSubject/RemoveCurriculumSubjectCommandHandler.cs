using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Subjects.Commands.RemoveCurriculumSubject;

public sealed class RemoveCurriculumSubjectCommandHandler : IRequestHandler<RemoveCurriculumSubjectCommand, Result>
{
    private readonly ICurriculumSubjectRepository _curriculum;
    private readonly ISubjectBookRepository _books;

    public RemoveCurriculumSubjectCommandHandler(ICurriculumSubjectRepository curriculum, ISubjectBookRepository books)
    {
        _curriculum = curriculum;
        _books = books;
    }

    public async Task<Result> Handle(RemoveCurriculumSubjectCommand request, CancellationToken ct)
    {
        var curriculumSubject = await _curriculum.GetByIdAsync(request.CurriculumSubjectId, ct);
        if (curriculumSubject is null || curriculumSubject.SchoolId != request.SchoolId)
            return Result.Failure("The grade subject assignment was not found.");

        foreach (var book in await _books.GetByCurriculumSubjectAsync(curriculumSubject.Id, ct))
            _books.Remove(book);
        _curriculum.Remove(curriculumSubject);
        return Result.Success();
    }
}

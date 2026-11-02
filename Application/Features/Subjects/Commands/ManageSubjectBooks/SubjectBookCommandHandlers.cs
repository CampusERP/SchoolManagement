using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Academics;
using MediatR;

namespace Application.Features.Subjects.Commands.ManageSubjectBooks;

public sealed class CreateSubjectBookCommandHandler : IRequestHandler<CreateSubjectBookCommand, Result<Guid>>
{
    private readonly ICurriculumSubjectRepository _curriculum;
    private readonly ISubjectBookRepository _books;
    public CreateSubjectBookCommandHandler(ICurriculumSubjectRepository curriculum, ISubjectBookRepository books)
    {
        _curriculum = curriculum;
        _books = books;
    }

    public async Task<Result<Guid>> Handle(CreateSubjectBookCommand request, CancellationToken ct)
    {
        var parent = await _curriculum.GetByIdAsync(request.CurriculumSubjectId, ct);
        if (parent is null || parent.SchoolId != request.SchoolId)
            return Result.Failure<Guid>("Subject is not assigned to this grade in this school.");

        var book = SubjectBook.Create(request.SchoolId, parent.Id, request.Title,
            request.Author, request.Publisher, request.Isbn, request.Edition, request.Notes);
        await _books.AddAsync(book, ct);
        return Result.Success(book.Id);
    }
}

public sealed class UpdateSubjectBookCommandHandler : IRequestHandler<UpdateSubjectBookCommand, Result>
{
    private readonly ISubjectBookRepository _books;
    public UpdateSubjectBookCommandHandler(ISubjectBookRepository books) => _books = books;

    public async Task<Result> Handle(UpdateSubjectBookCommand request, CancellationToken ct)
    {
        var book = await _books.GetByIdAndCurriculumAsync(request.BookId, request.CurriculumSubjectId, ct);
        if (book is null || book.SchoolId != request.SchoolId)
            return Result.Failure("Book not found.");

        try
        {
            book.Update(request.Title, request.Author, request.Publisher, request.Isbn, request.Edition, request.Notes);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}

public sealed class DeleteSubjectBookCommandHandler : IRequestHandler<DeleteSubjectBookCommand, Result>
{
    private readonly ISubjectBookRepository _books;
    public DeleteSubjectBookCommandHandler(ISubjectBookRepository books) => _books = books;

    public async Task<Result> Handle(DeleteSubjectBookCommand request, CancellationToken ct)
    {
        var book = await _books.GetByIdAndCurriculumAsync(request.BookId, request.CurriculumSubjectId, ct);
        if (book is null || book.SchoolId != request.SchoolId)
            return Result.Failure("Book not found.");
        _books.Remove(book);
        return Result.Success();
    }
}

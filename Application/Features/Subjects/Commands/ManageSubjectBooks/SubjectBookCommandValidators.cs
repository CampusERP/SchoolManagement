using FluentValidation;

namespace Application.Features.Subjects.Commands.ManageSubjectBooks;

public sealed class CreateSubjectBookCommandValidator : AbstractValidator<CreateSubjectBookCommand>
{
    public CreateSubjectBookCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.CurriculumSubjectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Author).MaximumLength(150);
        RuleFor(x => x.Publisher).MaximumLength(150);
        RuleFor(x => x.Isbn).MaximumLength(30);
        RuleFor(x => x.Edition).MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class UpdateSubjectBookCommandValidator : AbstractValidator<UpdateSubjectBookCommand>
{
    public UpdateSubjectBookCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.CurriculumSubjectId).NotEmpty();
        RuleFor(x => x.BookId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Author).MaximumLength(150);
        RuleFor(x => x.Publisher).MaximumLength(150);
        RuleFor(x => x.Isbn).MaximumLength(30);
        RuleFor(x => x.Edition).MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class DeleteSubjectBookCommandValidator : AbstractValidator<DeleteSubjectBookCommand>
{
    public DeleteSubjectBookCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.CurriculumSubjectId).NotEmpty();
        RuleFor(x => x.BookId).NotEmpty();
    }
}

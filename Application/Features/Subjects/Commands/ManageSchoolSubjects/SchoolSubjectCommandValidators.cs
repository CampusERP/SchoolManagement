using FluentValidation;

namespace Application.Features.Subjects.Commands.ManageSchoolSubjects;

public sealed class CreateSchoolSubjectCommandValidator : AbstractValidator<CreateSchoolSubjectCommand>
{
    public CreateSchoolSubjectCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.GradeLevelId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20).Matches("^[A-Za-z0-9]+$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateSchoolSubjectCommandValidator : AbstractValidator<UpdateSchoolSubjectCommand>
{
    public UpdateSchoolSubjectCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.CurriculumSubjectId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20).Matches("^[A-Za-z0-9]+$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class DeleteSchoolSubjectCommandValidator : AbstractValidator<DeleteSchoolSubjectCommand>
{
    public DeleteSchoolSubjectCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
    }
}

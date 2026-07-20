using FluentValidation;

namespace Application.Features.Exams.Commands;

public class RecordExamResultsCommandValidator : AbstractValidator<RecordExamResultsCommand>
{
    public RecordExamResultsCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.ExamScheduleId).NotEmpty();
        RuleFor(x => x.Results).NotEmpty()
            .WithMessage("At least one result entry is required.");
        RuleForEach(x => x.Results).ChildRules(r =>
        {
            r.RuleFor(x => x.StudentEnrollmentId).NotEmpty();
            r.RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
        });
    }
}

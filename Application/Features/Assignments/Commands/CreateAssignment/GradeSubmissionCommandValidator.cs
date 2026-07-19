using FluentValidation;
namespace Application.Features.Assignments.Commands.CreateAssignment;

public class GradeSubmissionCommandValidator : AbstractValidator<GradeSubmissionCommand>
{
    public GradeSubmissionCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.AssignmentId).NotEmpty();
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.Grade).GreaterThanOrEqualTo(0);
    }
}

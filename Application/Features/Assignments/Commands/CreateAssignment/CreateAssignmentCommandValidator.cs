using FluentValidation;

namespace Application.Features.Assignments.Commands.CreateAssignment;

public class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.TeachingAssignmentId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");
        RuleFor(x => x.MaxScore).GreaterThan(0).When(x => x.MaxScore.HasValue);
    }
}

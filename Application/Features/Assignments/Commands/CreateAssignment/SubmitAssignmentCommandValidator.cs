using FluentValidation;

namespace Application.Features.Assignments.Commands.CreateAssignment;

public class SubmitAssignmentCommandValidator : AbstractValidator<SubmitAssignmentCommand>
{
    public SubmitAssignmentCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.AssignmentId).NotEmpty();
        RuleFor(x => x.StudentEnrollmentId).NotEmpty();
    }
}

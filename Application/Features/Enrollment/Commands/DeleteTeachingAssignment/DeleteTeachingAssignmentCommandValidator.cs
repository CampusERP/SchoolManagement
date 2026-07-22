using FluentValidation;

namespace Application.Features.Enrollment.Commands.DeleteTeachingAssignment;

public class DeleteTeachingAssignmentCommandValidator : AbstractValidator<DeleteTeachingAssignmentCommand>
{
    public DeleteTeachingAssignmentCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.TeachingAssignmentId).NotEmpty();
    }
}

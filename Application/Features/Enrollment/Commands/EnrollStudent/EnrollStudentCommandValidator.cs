using FluentValidation;

namespace Application.Features.Enrollment.Commands.EnrollStudent;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required.");

        RuleFor(x => x.ClassRoomId)
            .NotEmpty().WithMessage("ClassRoom ID is required.");

        RuleFor(x => x.AcademicYearId)
            .NotEmpty().WithMessage("AcademicYear ID is required.");
    }
}

using FluentValidation;

namespace Application.Features.Academics.CreateClassRoom;

public class CreateClassRoomCommandValidator : AbstractValidator<CreateClassRoomCommand>
{
    public CreateClassRoomCommandValidator()
    {
        RuleFor(x => x.GradeLevelId)
            .NotEmpty().WithMessage("GradeLevel ID is required.");

        RuleFor(x => x.AcademicYearId)
            .NotEmpty().WithMessage("AcademicYear ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);
    }
}

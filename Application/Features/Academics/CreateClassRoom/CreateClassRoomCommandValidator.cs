using FluentValidation;

namespace Application.Features.Academics.CreateClassRoom;

public class CreateClassRoomCommandValidator : AbstractValidator<CreateClassRoomCommand>
{
    public CreateClassRoomCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.GradeLevelId).NotEmpty();
        RuleFor(x => x.AcademicYearId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
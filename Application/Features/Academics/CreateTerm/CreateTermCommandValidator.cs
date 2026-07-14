using FluentValidation;

namespace Application.Features.Academics.CreateTerm;

public class CreateTermCommandValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.AcademicYearId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Sequence).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .WithMessage("Term end date must be after start date.");
    }
}
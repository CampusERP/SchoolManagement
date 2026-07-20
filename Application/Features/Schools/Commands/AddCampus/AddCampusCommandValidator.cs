using FluentValidation;

namespace Application.Features.Schools.Commands.AddCampus;

public class AddCampusCommandValidator : AbstractValidator<AddCampusCommand>
{
    public AddCampusCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
    }
}
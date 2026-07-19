using FluentValidation;

namespace Application.Features.Schools.Commands.UpdateSchool;

public class UpdateSchoolCommandValidator : AbstractValidator<UpdateSchoolCommand>
{
    public UpdateSchoolCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
using FluentValidation;

namespace Application.Features.Billing.Commands;

public class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PriceMonthly).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxStudents).GreaterThan(0);
        RuleFor(x => x.MaxTeachers).GreaterThan(0);
    }
}

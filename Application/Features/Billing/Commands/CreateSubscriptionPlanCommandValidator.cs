using FluentValidation;

namespace Application.Features.Billing.Commands;

public class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DescriptionEn).MaximumLength(500);
        RuleFor(x => x.DescriptionAr).MaximumLength(500);
        RuleFor(x => x.PriceMonthly).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxStudents).GreaterThan(0);
        RuleFor(x => x.MaxTeachers).GreaterThan(0);
        RuleFor(x => x.Benefits).Must(HaveValidBenefitCount)
            .WithMessage("A plan can have at most 50 benefits.");

        RuleForEach(x => x.Benefits).ChildRules(benefit =>
        {
            benefit.RuleFor(b => b.DescriptionEn).MaximumLength(500);
            benefit.RuleFor(b => b.DescriptionAr).MaximumLength(500);
            benefit.RuleFor(b => b)
                .Must(b => !string.IsNullOrWhiteSpace(b.DescriptionEn) ||
                           !string.IsNullOrWhiteSpace(b.DescriptionAr))
                .WithMessage("A benefit needs an English or Arabic description.");
        });
    }

    private static bool HaveValidBenefitCount(List<SubscriptionPlanBenefitInput>? benefits) =>
        benefits is null || benefits.Count <= 50;
}

public class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    public UpdateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DescriptionEn).MaximumLength(500);
        RuleFor(x => x.DescriptionAr).MaximumLength(500);
        RuleFor(x => x.PriceMonthly).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxStudents).GreaterThan(0);
        RuleFor(x => x.MaxTeachers).GreaterThan(0);
        RuleFor(x => x.Benefits).Must(HaveValidBenefitCount)
            .WithMessage("A plan can have at most 50 benefits.");

        RuleForEach(x => x.Benefits).ChildRules(benefit =>
        {
            benefit.RuleFor(b => b.DescriptionEn).MaximumLength(500);
            benefit.RuleFor(b => b.DescriptionAr).MaximumLength(500);
            benefit.RuleFor(b => b)
                .Must(b => !string.IsNullOrWhiteSpace(b.DescriptionEn) ||
                           !string.IsNullOrWhiteSpace(b.DescriptionAr))
                .WithMessage("A benefit needs an English or Arabic description.");
        });
    }

    private static bool HaveValidBenefitCount(List<SubscriptionPlanBenefitInput>? benefits) =>
        benefits is null || benefits.Count <= 50;
}

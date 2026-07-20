using FluentValidation;

namespace Application.Features.Notifications.Commands;

public class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
{
    public RegisterDeviceTokenCommandValidator()
    {
        RuleFor(x => x.Platform).NotEmpty()
            .Must(p => new[] { "iOS", "Android", "Web" }.Contains(p))
            .WithMessage("Platform must be iOS, Android, or Web.");
        RuleFor(x => x.Token).NotEmpty();
    }
}

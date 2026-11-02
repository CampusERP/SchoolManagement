using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.ForgotPassword;

public class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailNotifier _emailNotifier;

    public ForgotPasswordCommandHandler(
        IIdentityService identityService,
        IEmailNotifier emailNotifier)
    {
        _identityService = identityService;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _identityService.GetByEmailAsync(request.Email, ct);

        if (user is not null)
        {
            var token = await _identityService.GeneratePasswordResetTokenAsync(user.Id, ct);

            var resetLink =
                $"{_emailNotifier.AppBaseUrl}/auth/reset-password" +
                $"?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}";

            await _emailNotifier.SendToEmailAsync(
                request.Email,
                EmailTemplate.PasswordReset,
                new Dictionary<string, string?>
                {
                    ["Name"] = user.Email,
                    ["Email"] = user.Email,
                    ["ResetLink"] = resetLink,
                    ["ExpiryHours"] = "24"
                },
                ct);
        }

        // Always report success to prevent account enumeration.
        return Result.Success();
    }
}

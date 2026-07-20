using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.ForgotPassword;

public class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IIdentityService identityService,
        IEmailService emailService)
    {
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _identityService.GetByEmailAsync(request.Email, ct);

        if (user is not null)
        {
            var token = await _identityService.GeneratePasswordResetTokenAsync(user.Id, ct);

            var resetLink = $"https://campuserpt.vercel.app/auth/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}";

            var subject = "Reset Your Password";
            var body = $"""
                <h2>Password Reset Request</h2>
                <p>You requested a password reset for your School Management account.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href="{resetLink}">Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not request this, please ignore this email.</p>
                """;

            await _emailService.SendAsync(user.Id, subject, body, ct);
        }

        return Result.Success();
    }
}

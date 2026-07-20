using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.ResetPassword;

public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _identityService.GetByEmailAsync(request.Email, ct);
        if (user is null)
            return Result.Failure("Invalid reset request.");

        var result = await _identityService.ResetPasswordAsync(
            user.Id, request.Token, request.NewPassword, ct);

        return result;
    }
}

using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IJwtTokenService _jwtService;

    public RefreshTokenCommandHandler(IJwtTokenService jwtService)
    {
        _jwtService = jwtService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        try
        {
            var tokens = await _jwtService.RefreshAsync(request.Token, ct);
            return Result.Success(new RefreshTokenResponse(
                tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiry));
        }
        catch (Exception ex)
        {
            return Result.Failure<RefreshTokenResponse>(ex.Message);
        }
    }
}

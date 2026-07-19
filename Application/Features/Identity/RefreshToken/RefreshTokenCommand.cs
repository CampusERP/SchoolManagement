using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.RefreshToken;

public record RefreshTokenCommand(string Token) : ICommand<RefreshTokenResponse>;

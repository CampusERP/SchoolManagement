using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Identity.Login;
using Application.Features.Identity.RefreshToken;
using Application.Features.Identity.Register;

namespace Api.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>Use refresh token to get a new access token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>Register a School Admin for an existing school. Platform Admin only.</summary>
    [HttpPost("register-school-admin")]
    [Authorize(Policy = "School.Create")]
    public async Task<IActionResult> RegisterSchoolAdmin(
        [FromBody] RegisterSchoolAdminCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created(result);
    }
}

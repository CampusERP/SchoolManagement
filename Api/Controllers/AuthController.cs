using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Identity.Login;
using Application.Features.Identity.RefreshToken;
using Application.Features.Identity.Register;
using Application.Features.Identity.SwitchSchool;
using Application.Features.Identity.ForgotPassword;
using Application.Features.Identity.ResetPassword;

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

    [HttpGet("memberships")]
    [Authorize]
    public async Task<IActionResult> GetMemberships(CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetUserMembershipsQuery(), ct));

    [HttpPost("switch-school")]
    [Authorize]
    public async Task<IActionResult> SwitchSchool(
        [FromBody] SwitchSchoolCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command, ct));

    /// <summary>
    /// Request a password reset link. Always returns success to prevent email enumeration.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(new { message = "If an account with that email exists, a reset link has been sent." });
    }

    /// <summary>
    /// Reset password using the token from the email link.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command, ct));
}

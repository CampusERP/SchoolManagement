using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult(Result result) =>
        result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });

    protected IActionResult FromResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });

    protected IActionResult Created<T>(Result<T> result, string? locationPath = null) =>
        result.IsSuccess
            ? StatusCode(201, new { id = result.Value })
            : BadRequest(new { error = result.Error });
}
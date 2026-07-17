using Microsoft.AspNetCore.Http;
using Application.Common.Interfaces.Services;

namespace Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public IReadOnlyCollection<string> Permissions =>
        _httpContextAccessor.HttpContext?.User?
            .FindAll("permission")
            .Select(c => c.Value)
            .ToList() ?? new List<string>();
}

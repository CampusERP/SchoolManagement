using Microsoft.AspNetCore.Http;
using Application.Common.Interfaces;

namespace Infrastructure.Identity;

/// <summary>
/// Reads the active tenant off the current HTTP request's JWT claims.
/// "school_id" claim is absent for platform admin tokens — that's the
/// signal IsPlatformAdmin uses, backed up by an explicit "is_platform_admin"
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? SchoolId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst("school_id")?.Value;
            return Guid.TryParse(value, out var schoolId) ? schoolId : null;
        }
    }

    public bool IsPlatformAdmin =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("is_platform_admin")?.Value == "true";
}

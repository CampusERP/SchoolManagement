namespace Application.Common.Constants;

/// <summary>
/// User-facing messages explaining why a school's users are locked out.
/// Kept in one place so the middleware and the identity handlers stay consistent.
/// </summary>
public static class SchoolAccessMessages
{
    public const string Suspended =
        "This school has been suspended. Please contact the platform administrator.";
}

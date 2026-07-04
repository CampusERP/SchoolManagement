namespace Application.Common.Exceptions;

/// <summary>
/// Thrown when a handler determines that the current user does not have permission to access a resource.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("You do not have permission to access this resource.") { }

    public ForbiddenException(string message) : base(message) { }
}

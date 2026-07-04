namespace Application.Common.Exceptions;

/// <summary>
/// Thrown when a handler determines that a requested entity was not found in the database.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}

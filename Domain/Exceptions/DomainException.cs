namespace Domain.Exceptions;

/// <summary>
/// Represents an exception that occurs in the domain layer of the application.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

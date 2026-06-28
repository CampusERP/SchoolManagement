namespace Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated (e.g., withdrawing an
/// already-withdrawn enrollment, double-booking a teacher). Caught by
/// global exception-handling middleware in Api and mapped to HTTP 400.
/// Never throw a generic Exception from inside an entity — always this.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

namespace Application.Common.Models;

public record SignInAttempt(SignInStatus Status, AuthenticatedUser? User)
{
    public static SignInAttempt Success(AuthenticatedUser user) => new(SignInStatus.Success, user);
    public static SignInAttempt InvalidCredentials() => new(SignInStatus.InvalidCredentials, null);
    public static SignInAttempt LockedOut() => new(SignInStatus.LockedOut, null);
}
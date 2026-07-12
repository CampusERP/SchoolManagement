namespace Application.Common.Models;

public record AuthenticatedUser(Guid Id, string Email, bool IsPlatformAdmin);
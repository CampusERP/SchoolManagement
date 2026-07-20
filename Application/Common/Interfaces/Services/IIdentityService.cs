using Application.Common.Models;

namespace Application.Common.Interfaces.Services;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(string email, string password, CancellationToken ct);
    Task<Result> AddToRoleAsync(Guid userId, string role, CancellationToken ct);
    Task DeleteUserAsync(Guid userId, CancellationToken ct);
    Task<SignInAttempt> ValidateCredentialsAsync(string email, string password, CancellationToken ct);
    Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct);
    Task<AuthenticatedUser?> GetByIdAsync(Guid userId, CancellationToken ct);
    Task<AuthenticatedUser?> GetByEmailAsync(string email, CancellationToken ct);
    Task<string> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken ct);
    Task<Result> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct);
}
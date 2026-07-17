using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ── Validate password + lockout ────────────────────────────────────
    public async Task<SignInAttempt> ValidateCredentialsAsync(
        string email, string password, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return SignInAttempt.InvalidCredentials();

        // lockoutOnFailure: true — increments failed count, locks after threshold
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (result.IsLockedOut) return SignInAttempt.LockedOut();
        if (!result.Succeeded) return SignInAttempt.InvalidCredentials();

        return SignInAttempt.Success(new AuthenticatedUser(user.Id, user.Email!, user.IsPlatformAdmin));
    }

    // ── Create user — Identity calls SaveChanges internally ────────────
    public async Task<Result<Guid>> CreateUserAsync(
        string email, string password, CancellationToken ct)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return Result.Failure<Guid>($"A user with email '{email}' already exists.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return Result.Failure<Guid>(string.Join("; ", result.Errors.Select(e => e.Description)));

        return Result.Success(user.Id);
    }

    // ── Assign role — Identity calls SaveChanges internally ───────────
    public async Task<Result> AddToRoleAsync(Guid userId, string role, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure("User not found.");

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            return Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

        return Result.Success();
    }

    // ── Compensation — called when downstream steps fail ───────────────
    public async Task DeleteUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
            await _userManager.DeleteAsync(user);
    }

    // ── Roles ─────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Array.Empty<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList().AsReadOnly();
    }

    public async Task<AuthenticatedUser?> GetByIdAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return null;

        return new AuthenticatedUser(user.Id, user.Email!, user.IsPlatformAdmin);
    }
}
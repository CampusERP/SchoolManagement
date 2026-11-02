using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.ResetStudentPassword;

public sealed class ResetStudentPasswordCommandHandler : IRequestHandler<ResetStudentPasswordCommand, Result>
{
    private readonly IStudentRepository _students;
    private readonly IIdentityService _identity;
    private readonly IEmailNotifier _email;

    public ResetStudentPasswordCommandHandler(IStudentRepository students, IIdentityService identity, IEmailNotifier email)
    {
        _students = students;
        _identity = identity;
        _email = email;
    }

    public async Task<Result> Handle(ResetStudentPasswordCommand request, CancellationToken ct)
    {
        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null || student.SchoolId != request.SchoolId || student.ApplicationUserId is not Guid userId)
            return Result.Failure("Student login was not found.");

        var user = await _identity.GetByIdAsync(userId, ct);
        if (user is null) return Result.Failure("Student login was not found.");

        var token = await _identity.GeneratePasswordResetTokenAsync(userId, ct);
        var resetLink = $"{_email.AppBaseUrl}/auth/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
        await _email.SendToEmailAsync(user.Email, EmailTemplate.PasswordReset,
            new Dictionary<string, string?>
            {
                ["Name"] = $"{student.FirstName} {student.LastName}".Trim(),
                ["Email"] = user.Email,
                ["ResetLink"] = resetLink,
                ["ExpiryHours"] = "24"
            }, ct);

        return Result.Success();
    }
}

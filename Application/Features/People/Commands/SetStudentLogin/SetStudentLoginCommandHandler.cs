using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.SetStudentLogin;

public sealed class SetStudentLoginCommandHandler : IRequestHandler<SetStudentLoginCommand, Result>
{
    private readonly IStudentRepository _students;
    private readonly ISchoolRepository _schools;
    private readonly IIdentityService _identity;
    private readonly IOutboxService _outbox;
    private readonly IEmailNotifier _email;

    public SetStudentLoginCommandHandler(
        IStudentRepository students,
        ISchoolRepository schools,
        IIdentityService identity,
        IOutboxService outbox,
        IEmailNotifier email)
    {
        _students = students;
        _schools = schools;
        _identity = identity;
        _outbox = outbox;
        _email = email;
    }

    public async Task<Result> Handle(SetStudentLoginCommand request, CancellationToken ct)
    {
        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null || student.SchoolId != request.SchoolId)
            return Result.Failure("Student not found.");

        if (!request.Enabled)
        {
            if (student.ApplicationUserId is Guid existingUserId && student.LoginEnabled)
            {
                var disabled = await _identity.SetLoginEnabledAsync(existingUserId, false, ct);
                if (disabled.IsFailure) return disabled;
            }

            student.SetLoginEnabled(false);
            return Result.Success();
        }

        if (student.ApplicationUserId is Guid linkedUserId)
        {
            if (!student.LoginEnabled)
            {
                var enabled = await _identity.SetLoginEnabledAsync(linkedUserId, true, ct);
                if (enabled.IsFailure) return enabled;
            }

            student.SetLoginEnabled(true);
            return Result.Success();
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure("Email and password are required to create a student login.");

        var created = await _identity.CreateUserAsync(request.Email.Trim(), request.Password, ct);
        if (created.IsFailure) return Result.Failure(created.Error ?? "Failed to create user account.");

        var userId = created.Value;
        try
        {
            var role = await _identity.AddToRoleAsync(userId, "Student", ct);
            if (role.IsFailure)
            {
                await _identity.DeleteUserAsync(userId, ct);
                return role;
            }

            student.LinkLogin(userId);
            student.SetLoginEnabled(true);
            _outbox.Publish(new LinkStudentLoginMessage(student.Id, userId));

            var school = await _schools.GetByIdAsync(request.SchoolId, ct);
            await _email.SendAsync(userId, EmailTemplate.Welcome,
                new Dictionary<string, string?>
                {
                    ["Name"] = string.Join(" ", new[] { student.FirstName, student.SecondName, student.ThirdName, student.LastName }
                        .Where(name => !string.IsNullOrWhiteSpace(name))),
                    ["StudentCode"] = student.StudentCode,
                    ["Role"] = "Student",
                    ["SchoolName"] = school?.Name ?? "your school",
                    ["Email"] = request.Email.Trim(),
                    ["Password"] = request.Password,
                    ["LoginUrl"] = $"{_email.AppBaseUrl}/auth/login"
                }, ct);

            return Result.Success();
        }
        catch
        {
            await _identity.DeleteUserAsync(userId, ct);
            throw;
        }
    }
}

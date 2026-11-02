using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Application.Common.Models;
using Domain.Entities.People;
using MediatR;

namespace Application.Features.People.Commands.CreateTeacher;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Result<Guid>>
{
    private readonly ITeacherRepository _teachers;
    private readonly ISchoolRepository _schools;
    private readonly IIdentityService _identityService;
    private readonly IOutboxService _outbox;
    private readonly IEmailNotifier _emailNotifier;

    public CreateTeacherCommandHandler(
        ITeacherRepository teachers,
        ISchoolRepository schools,
        IIdentityService identityService,
        IOutboxService outbox,
        IEmailNotifier emailNotifier)
    {
        _teachers = teachers;
        _schools = schools;
        _identityService = identityService;
        _outbox = outbox;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result<Guid>> Handle(CreateTeacherCommand request, CancellationToken ct)
    {
        var exists = await _teachers.ExistsAsync(request.SchoolId, request.EmployeeCode, ct: ct);
        if (exists)
            return Result.Failure<Guid>($"A teacher with employee code '{request.EmployeeCode}' already exists in this school.");

        var userResult = await _identityService.CreateUserAsync(request.Email, request.Password, ct);
        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error ?? "Failed to create user account.");

        var userId = userResult.Value;

        try
        {
            await _identityService.AddToRoleAsync(userId, "Teacher", ct);

            var teacher = Teacher.Create(
                request.SchoolId, userId, request.EmployeeCode,
                request.FirstName, request.LastName);

            await _teachers.AddAsync(teacher, ct);

            _outbox.Publish(new LinkTeacherLoginMessage(teacher.Id, userId));

            var school = await _schools.GetByIdAsync(request.SchoolId, ct);
            await _emailNotifier.SendAsync(
                userId,
                EmailTemplate.Welcome,
                new Dictionary<string, string?>
                {
                    ["Name"] = $"{request.FirstName} {request.LastName}".Trim(),
                    ["Role"] = "Teacher",
                    ["SchoolName"] = school?.Name ?? "your school",
                    ["Email"] = request.Email,
                    ["Password"] = request.Password,
                    ["LoginUrl"] = $"{_emailNotifier.AppBaseUrl}/auth/login"
                },
                ct);

            return Result.Success(teacher.Id);
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId, ct);
            throw;
        }

    }
}

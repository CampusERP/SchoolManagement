using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Application.Common.Models;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
using MediatR;
using System.Security.Cryptography;

namespace Application.Features.People.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _students;
    private readonly ISchoolRepository _schools;
    private readonly IIdentityService _identityService;
    private readonly IOutboxService _outbox;
    private readonly IEmailNotifier _emailNotifier;

    public CreateStudentCommandHandler(
        IStudentRepository students,
        ISchoolRepository schools,
        IIdentityService identityService,
        IOutboxService outbox,
        IEmailNotifier emailNotifier)
    {
        _students = students;
        _schools = schools;
        _identityService = identityService;
        _outbox = outbox;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken ct)
    {
        var studentCode = request.StudentCode;
        if (string.IsNullOrWhiteSpace(studentCode))
        {
            do
            {
                studentCode = $"STU-{Convert.ToHexString(RandomNumberGenerator.GetBytes(5))}";
            } while (await _students.ExistsAsync(request.SchoolId, studentCode, ct: ct));
        }
        else if (await _students.ExistsAsync(request.SchoolId, studentCode, ct: ct))
        {
            return Result.Failure<Guid>($"A student with code '{studentCode}' already exists in this school.");
        }

        var student = Student.Create(request.SchoolId, studentCode, request.FirstName, request.LastName, request.DateOfBirth, request.NationalId);

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Password))
        {
                
            var user = await _identityService.CreateUserAsync(request.Email, request.Password, ct);
            if (!user.IsSuccess)
                return Result.Failure<Guid>(user.Error ?? "Failed to create user account.");

            var userId = user.Value;

            try
            {
                await _identityService.AddToRoleAsync(userId, "Student", ct);
                student.LinkLogin(userId);
                await _students.AddAsync(student, ct);
                _outbox.Publish(new LinkStudentLoginMessage(student.Id, userId));

                var school = await _schools.GetByIdAsync(request.SchoolId, ct);
                await _emailNotifier.SendAsync(
                    userId,
                    EmailTemplate.Welcome,
                    new Dictionary<string, string?>
                    {
                        ["Name"] = $"{request.FirstName} {request.LastName}".Trim(),
                        ["StudentCode"] = studentCode,
                        ["Role"] = "Student",
                        ["SchoolName"] = school?.Name ?? "your school",
                        ["Email"] = request.Email!,
                        ["Password"] = request.Password!,
                        ["LoginUrl"] = $"{_emailNotifier.AppBaseUrl}/auth/login"
                    },
                    ct);

                return Result.Success(student.Id);
            }
            catch
            {
                await _identityService.DeleteUserAsync(userId, ct);
                throw;
            }
        }
        await _students.AddAsync(student, ct);
        return Result.Success(student.Id);
    }
}

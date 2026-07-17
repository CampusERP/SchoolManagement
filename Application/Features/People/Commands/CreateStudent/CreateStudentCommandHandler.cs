using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
using Application.Common.Interfaces.Services;

namespace Application.Features.People.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _students;
    private readonly IIdentityService _identityService;
    private readonly IUserSchoolMembershipRepository _memberships;

    public CreateStudentCommandHandler(
        IStudentRepository students,
        IIdentityService identityService,
        IUserSchoolMembershipRepository memberships)
    {
        _students = students;
        _identityService = identityService;
        _memberships = memberships;
    }

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken ct)
    {
        var exists = await _students.ExistsAsync(request.SchoolId, request.StudentCode, ct: ct);
        if (exists)
            return Result.Failure<Guid>($"A student with code '{request.StudentCode}' already exists in this school.");

        var student = Student.Create(request.SchoolId, request.StudentCode, request.FirstName, request.LastName, request.DateOfBirth);

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Password))
        {
                
            var user = await _identityService.CreateUserAsync(request.Email, request.Password, ct);
            if (!user.IsSuccess)
                return Result.Failure<Guid>(user.Error ?? "Failed to create user account.");

            var userId = user.Value;

            try
            {
                var roleResult = await _identityService.AddToRoleAsync(userId, "Student", ct);
                if (!roleResult.IsSuccess)
                {
                    await _identityService.DeleteUserAsync(userId, ct);
                    return Result.Failure<Guid>(roleResult.Error!);
                }

                await _memberships.AddAsync(UserSchoolMembership.Create(userId, request.SchoolId), ct);
                student.LinkLogin(userId);
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

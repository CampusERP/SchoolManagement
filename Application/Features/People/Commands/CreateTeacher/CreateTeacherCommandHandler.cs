using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
using Application.Common.Interfaces.Services;

namespace Application.Features.People.Commands.CreateTeacher;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Result<Guid>>
{
    private readonly ITeacherRepository _teachers;
    private readonly IIdentityService _identityService;
    private readonly IUserSchoolMembershipRepository _memberships;

    public CreateTeacherCommandHandler(
        ITeacherRepository teachers,
        IIdentityService identityService,
        IUserSchoolMembershipRepository memberships)
    {
        _teachers = teachers;
        _identityService = identityService;
        _memberships = memberships;
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
            var roleResult = await _identityService.AddToRoleAsync(userId, "Teacher", ct);
            if (roleResult.IsFailure)
            {
                await _identityService.DeleteUserAsync(userId, ct);
                return Result.Failure<Guid>(roleResult.Error ?? "Failed to assign the Teacher role.");
            }

            var teacher = Teacher.Create(request.SchoolId, userId, request.EmployeeCode, request.FirstName, request.LastName);
            await _memberships.AddAsync(UserSchoolMembership.Create(userId, request.SchoolId), ct);
            await _teachers.AddAsync(teacher, ct);
            return Result.Success(teacher.Id);
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId, ct);
            throw;
        }

    }
}

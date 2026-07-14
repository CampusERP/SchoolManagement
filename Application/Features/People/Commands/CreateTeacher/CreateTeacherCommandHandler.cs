using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;

namespace Application.Features.People.Commands.CreateTeacher;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Result<Guid>>
{
    private readonly ITeacherRepository _teachers;
    private readonly IIdentityService _identityService;

    public CreateTeacherCommandHandler(ITeacherRepository teachers, IIdentityService identityService)
    {
        _teachers = teachers;
        _identityService = identityService;
    }

    public async Task<Result<Guid>> Handle(CreateTeacherCommand request, CancellationToken ct)
    {
        var exists = await _teachers.ExistsAsync(request.SchoolId, request.EmployeeCode, ct);
        if (exists)
            return Result.Failure<Guid>($"A teacher with employee code '{request.EmployeeCode}' already exists in this school.");

        var userResult = await _identityService.CreateUserAsync(request.Email, request.Password, ct);
        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error);

        var userId = userResult.Value;

        try
        {
            var roleResult = await _identityService.AddToRoleAsync(userId, "Teacher", ct);
            if (roleResult.IsFailure)
                return Result.Failure<Guid>(roleResult.Error);

            var teacher = Teacher.Create(request.SchoolId, userId, request.EmployeeCode, request.FirstName, request.LastName);
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
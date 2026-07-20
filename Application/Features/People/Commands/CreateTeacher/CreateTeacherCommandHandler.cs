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
    private readonly IIdentityService _identityService;
    private readonly IOutboxService _outbox;

    public CreateTeacherCommandHandler(
        ITeacherRepository teachers,
        IIdentityService identityService,
        IOutboxService outbox)
    {
        _teachers = teachers;
        _identityService = identityService;
        _outbox = outbox;
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

            return Result.Success(teacher.Id);
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId, ct);
            throw;
        }

    }
}

using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Application.Common.Models;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.People.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _students;
    private readonly IIdentityService _identityService;
    private readonly IOutboxService _outbox;

    public CreateStudentCommandHandler(
        IStudentRepository students,
        IIdentityService identityService,
        IOutboxService outbox)
    {
        _students = students;
        _identityService = identityService;
        _outbox = outbox;
    }

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken ct)
    {
        var exists = await _students.ExistsAsync(request.SchoolId, request.StudentCode, ct: ct);
        if (exists)
            return Result.Failure<Guid>($"A student with code '{request.StudentCode}' already exists in this school.");

        var student = Student.Create(request.SchoolId, request.StudentCode, request.FirstName, request.LastName, request.DateOfBirth, request.NationalId);

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Password))
        {
                
            var user = await _identityService.CreateUserAsync(request.Email, request.Password, ct);
            if (!user.IsSuccess)
                return Result.Failure<Guid>(user.Error ?? "Failed to create user account.");

            var userId = user.Value;

            try
            {
                await _identityService.AddToRoleAsync(userId, "Student", ct);
                _outbox.Publish(new LinkStudentLoginMessage(student.Id, userId));
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
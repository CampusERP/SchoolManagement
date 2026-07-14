using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _students;
    private readonly IIdentityService _identityService;

    public CreateStudentCommandHandler(IStudentRepository students, IIdentityService identityService)
    {
        _students = students;
        _identityService = identityService;
    }

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken ct)
    {
        var exists = await _students.ExistsAsync(request.SchoolId, request.StudentCode, ct);
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
                    return Result.Failure<Guid>(roleResult.Error!);

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
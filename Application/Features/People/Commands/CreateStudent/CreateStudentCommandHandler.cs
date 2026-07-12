using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;

namespace Application.Features.People.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _students;
    private readonly ITenantContext _tenant;

    public CreateStudentCommandHandler(IStudentRepository students, ITenantContext tenant)
    {
        _students = students;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var exists = await _students.ExistsAsync(schoolId, request.StudentCode, ct);
        if (exists)
            return Result.Failure<Guid>($"A student with code '{request.StudentCode}' already exists in this school.");

        var student = Student.Create(schoolId, request.StudentCode, request.FirstName, request.LastName, request.DateOfBirth);
        await _students.AddAsync(student, ct);

        return Result.Success(student.Id);
    }
}

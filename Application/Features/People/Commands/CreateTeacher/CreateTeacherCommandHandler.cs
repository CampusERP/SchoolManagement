using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;

namespace Application.Features.People.Commands.CreateTeacher;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Result<Guid>>
{
    private readonly ITeacherRepository _teachers;
    private readonly ITenantContext _tenant;

    public CreateTeacherCommandHandler(ITeacherRepository teachers, ITenantContext tenant)
    {
        _teachers = teachers;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateTeacherCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var exists = await _teachers.ExistsAsync(schoolId, request.EmployeeCode, ct);
        if (exists)
            return Result.Failure<Guid>($"A teacher with employee code '{request.EmployeeCode}' already exists in this school.");

        var teacher = Teacher.Create(schoolId, request.ApplicationUserId, request.EmployeeCode, request.FirstName, request.LastName);
        await _teachers.AddAsync(teacher, ct);

        return Result.Success(teacher.Id);
    }
}

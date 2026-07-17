using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.UpdateTeacher;

public class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, Result>
{
    private readonly ITeacherRepository _teachers;

    public UpdateTeacherCommandHandler(ITeacherRepository teachers) => _teachers = teachers;

    public async Task<Result> Handle(UpdateTeacherCommand request, CancellationToken ct)
    {
        var teacher = await _teachers.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null || teacher.SchoolId != request.SchoolId)
            return Result.Failure("Teacher not found.");

        if (await _teachers.ExistsAsync(request.SchoolId, request.EmployeeCode, request.TeacherId, ct))
            return Result.Failure($"A teacher with employee code '{request.EmployeeCode}' already exists in this school.");

        try
        {
            teacher.Update(request.EmployeeCode, request.FirstName, request.LastName, request.EmploymentStatus);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}

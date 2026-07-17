using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result>
{
    private readonly IStudentRepository _students;

    public UpdateStudentCommandHandler(IStudentRepository students)
    {
        _students = students;
    }

    public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken ct)
    {
        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null || student.SchoolId != request.SchoolId)
            return Result.Failure("Student not found.");

        var codeInUse = await _students.ExistsAsync(
            request.SchoolId, request.StudentCode, request.StudentId, ct);
        if (codeInUse)
            return Result.Failure($"A student with code '{request.StudentCode}' already exists in this school.");

        try
        {
            student.Update(request.StudentCode, request.FirstName, request.LastName, request.DateOfBirth);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}

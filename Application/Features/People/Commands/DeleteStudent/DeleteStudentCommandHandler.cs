using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.DeleteStudent;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, Result>
{
    private readonly IStudentRepository _students;

    public DeleteStudentCommandHandler(IStudentRepository students)
    {
        _students = students;
    }

    public async Task<Result> Handle(DeleteStudentCommand request, CancellationToken ct)
    {
        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null || student.SchoolId != request.SchoolId)
            return Result.Failure("Student not found in this school.");

        await _students.RemoveAsync(student, ct);
        return Result.Success();
    }
}

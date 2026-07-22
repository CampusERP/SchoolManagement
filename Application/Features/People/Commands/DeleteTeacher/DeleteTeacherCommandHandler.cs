using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.DeleteTeacher;

public class DeleteTeacherCommandHandler : IRequestHandler<DeleteTeacherCommand, Result>
{
    private readonly ITeacherRepository _teachers;

    public DeleteTeacherCommandHandler(ITeacherRepository teachers)
    {
        _teachers = teachers;
    }

    public async Task<Result> Handle(DeleteTeacherCommand request, CancellationToken ct)
    {
        var teacher = await _teachers.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null || teacher.SchoolId != request.SchoolId)
            return Result.Failure("Teacher not found in this school.");

        await _teachers.RemoveAsync(teacher, ct);
        return Result.Success();
    }
}

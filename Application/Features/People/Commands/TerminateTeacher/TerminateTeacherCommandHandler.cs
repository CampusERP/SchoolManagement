using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.TerminateTeacher;

public class TerminateTeacherCommandHandler : IRequestHandler<TerminateTeacherCommand, Result>
{
    private readonly ITeacherRepository _teachers;
    public TerminateTeacherCommandHandler(ITeacherRepository teachers) => _teachers = teachers;

    public async Task<Result> Handle(TerminateTeacherCommand request, CancellationToken ct)
    {
        var teacher = await _teachers.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null) throw new NotFoundException("Teacher", request.TeacherId);
        teacher.Terminate();
        return Result.Success();
    }
}

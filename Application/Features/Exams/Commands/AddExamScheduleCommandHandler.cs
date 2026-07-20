using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Exams;
using MediatR;

namespace Application.Features.Exams.Commands;

public class AddExamScheduleCommandHandler : IRequestHandler<AddExamScheduleCommand, Result<Guid>>
{
    private readonly IExamRepository _exams;
    public AddExamScheduleCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<Guid>> Handle(AddExamScheduleCommand request, CancellationToken ct)
    {
        var exam = await _exams.GetByIdAsync(request.ExamId, ct);
        if (exam is null) throw new NotFoundException(nameof(Exam), request.ExamId);

        try
        {
            var schedule = exam.AddSchedule(request.ClassRoomId, request.RoomId, request.ExamDate);
            return Result.Success(schedule.Id);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }
    }
}

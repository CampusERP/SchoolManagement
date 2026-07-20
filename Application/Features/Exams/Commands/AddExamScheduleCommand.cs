using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Exams.Commands;

public record AddExamScheduleCommand(Guid SchoolId, Guid ExamId,
    Guid ClassRoomId, Guid RoomId, DateTime ExamDate)
    : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;

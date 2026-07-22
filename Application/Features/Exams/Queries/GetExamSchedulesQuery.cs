using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public record GetExamSchedulesQuery(Guid ExamId)
    : IRequest<Result<List<ExamScheduleDto>>>;

using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Queries;

public record GetClassExamResultsQuery(Guid SchoolId, Guid ExamScheduleId)
    : IRequest<Result<List<ClassExamResultDto>>>, ITenantScopedRequest;

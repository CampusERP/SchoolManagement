using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public record GetStudentScheduleQuery(Guid SchoolId, Guid StudentEnrollmentId, Guid TermId)
    : IRequest<Result<List<StudentScheduleSlotDto>>>, ITenantScopedRequest;

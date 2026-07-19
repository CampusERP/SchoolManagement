using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public record GetTeacherScheduleQuery(Guid SchoolId, Guid TeacherId, Guid TermId)
    : IRequest<Result<List<TeacherScheduleSlotDto>>>, ITenantScopedRequest;

using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetTeacherById;

public record GetTeacherByIdQuery(Guid SchoolId, Guid TeacherId)
    : IRequest<Result<TeacherDetailDto>>, ITenantScopedRequest;

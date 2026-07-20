using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetStudentClasses;

public record GetStudentClassesQuery(Guid SchoolId, Guid StudentEnrollmentId)
    : IRequest<Result<List<StudentClassDto>>>, ITenantScopedRequest;

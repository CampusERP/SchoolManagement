using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.StudentDetails;

public record GetStudentByIdQuery(Guid SchoolId, Guid StudentId)
    : IRequest<Result<StudentDetailDto>>, ITenantScopedRequest;
using Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Schools.Queries.GetSchoolById;

public record GetSchoolByIdQuery(Guid SchoolId) : IRequest<Result<SchoolDetailDto>>, ITenantScopedRequest;

using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Queries.GetSchoolById;

public record GetSchoolByIdQuery(Guid SchoolId) : IRequest<Result<SchoolDetailDto>>;

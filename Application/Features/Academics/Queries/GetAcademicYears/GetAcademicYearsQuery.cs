using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetAcademicYears;

public record GetAcademicYearsQuery(Guid SchoolId)
    : IRequest<Result<List<AcademicYearDto>>>, ITenantScopedRequest;

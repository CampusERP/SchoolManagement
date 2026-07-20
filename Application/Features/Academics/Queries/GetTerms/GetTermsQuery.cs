using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetTerms;

public record GetTermsQuery(Guid SchoolId, Guid AcademicYearId)
    : IRequest<Result<List<TermDto>>>, ITenantScopedRequest;

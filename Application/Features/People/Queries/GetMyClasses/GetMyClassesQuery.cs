using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.People.Queries.GetTeacherById;
using MediatR;

namespace Application.Features.People.Queries.GetMyClasses;

public record GetMyClassesQuery(Guid SchoolId, Guid TermId)
    : IRequest<Result<List<TeachingAssignmentSummaryDto>>>, ITenantScopedRequest;

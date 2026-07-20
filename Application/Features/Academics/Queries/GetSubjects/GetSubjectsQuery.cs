using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetSubjects;

public record GetSubjectsQuery(Guid? GradeLevelId = null)
    : IRequest<Result<List<SubjectDto>>>;
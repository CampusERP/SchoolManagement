using MediatR;
using Application.Common.Models;

namespace Application.Features.Academics.Queries.GetEducationStages;

public record GetEducationStagesQuery : IRequest<Result<List<EducationStageDto>>>;

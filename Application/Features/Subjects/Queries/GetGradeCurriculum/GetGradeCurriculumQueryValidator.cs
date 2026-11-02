using FluentValidation;

namespace Application.Features.Subjects.Queries.GetGradeCurriculum;

public sealed class GetGradeCurriculumQueryValidator : AbstractValidator<GetGradeCurriculumQuery>
{
    public GetGradeCurriculumQueryValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.GradeLevelId).NotEmpty();
    }
}

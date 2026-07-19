using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Tenant-scoped join entity: this school teaches this Subject at this GradeLevel.
/// "GradeSubject" was the original name — renamed because "GradeSubject"
/// reads as two unrelated nouns mashed together and will be misread by the
/// next developer who touches it. Names matter at this scale.
/// </summary>
public class CurriculumSubject : TenantEntity
{
    public Guid GradeLevelId { get; private set; }
    public Guid SubjectId    { get; private set; }

    private CurriculumSubject() { } // EF Core

    private CurriculumSubject(Guid id, Guid schoolId, Guid gradeLevelId, Guid subjectId)
        : base(id, schoolId)
    {
        GradeLevelId = gradeLevelId;
        SubjectId    = subjectId;
    }

    public static CurriculumSubject Create(Guid schoolId, Guid gradeLevelId, Guid subjectId)
        => new(Guid.NewGuid(), schoolId, gradeLevelId, subjectId);
}

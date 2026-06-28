using Domain.Common;

namespace Domain.Entities.Academics;

public class GradeLevel : TenantEntity, IAggregateRoot
{
    public Guid EducationStageId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Sequence { get; private set; }

    private GradeLevel() { } // EF Core

    private GradeLevel(Guid id, Guid schoolId, Guid educationStageId, string name, int sequence)
        : base(id, schoolId)
    {
        EducationStageId = educationStageId;
        Name = name;
        Sequence = sequence;
    }

    public static GradeLevel Create(Guid schoolId, Guid educationStageId, string name, int sequence)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new GradeLevel(Guid.NewGuid(), schoolId, educationStageId, name, sequence);
    }
}

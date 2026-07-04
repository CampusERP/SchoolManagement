using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Represents an academic term within a specific academic year.
/// </summary>
public class Term : AuditableEntity
{
    public Guid AcademicYearId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Sequence { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private Term() { } // EF Core

    private Term(Guid id, Guid academicYearId, string name, int sequence,
        DateTime startDate, DateTime endDate) : base(id)
    {
        AcademicYearId = academicYearId;
        Name = name;
        Sequence = sequence;
        StartDate = startDate;
        EndDate = endDate;
    }

    internal static Term Create(Guid academicYearId, string name, int sequence,
        DateTime startDate, DateTime endDate)
    {
        return new Term(Guid.NewGuid(), academicYearId, name, sequence, startDate, endDate);
    }
}

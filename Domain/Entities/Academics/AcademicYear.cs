using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Academics;

/// <summary>
/// Represents an academic year within a school, including its name, start and end dates, status, and associated terms.
/// </summary>
public class AcademicYear : TenantEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsCurrent { get; private set; }
    public AcademicYearStatus Status { get; private set; }

    private readonly List<Term> _terms = new();
    public IReadOnlyCollection<Term> Terms => _terms.AsReadOnly();

    private AcademicYear() { } // EF Core

    private AcademicYear(Guid id, Guid schoolId, string name, DateTime startDate, DateTime endDate)
        : base(id, schoolId)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Status = AcademicYearStatus.Planning;
        IsCurrent = false;
    }

    public static AcademicYear Create(Guid schoolId, string name, DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new DomainException("Academic year end date must be after the start date.");

        return new AcademicYear(Guid.NewGuid(), schoolId, name, startDate, endDate);
    }

    public Term AddTerm(string name, int sequence, DateTime startDate, DateTime endDate)
    {
        if (Status == AcademicYearStatus.Closed || Status == AcademicYearStatus.Archived)
            throw new DomainException("Cannot add a term to a closed or archived academic year.");

        if (startDate < StartDate || endDate > EndDate)
            throw new DomainException("Term dates must fall within the academic year's date range.");

        if (_terms.Any(t => sequence == t.Sequence))
            throw new DomainException($"A term with sequence {sequence} already exists.");

        var term = Term.Create(Id, name, sequence, startDate, endDate);
        _terms.Add(term);
        return term;
    }

    public void Update(string name, DateTime startDate, DateTime endDate)
    {
        if (Status == AcademicYearStatus.Closed || Status == AcademicYearStatus.Archived)
            throw new DomainException("Cannot update a closed or archived academic year.");
        if (endDate <= startDate)
            throw new DomainException("Academic year end date must be after the start date.");
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Activate()
    {
        IsCurrent = true;
        Status = AcademicYearStatus.Active;
    }

    public void Close()
    {
        if (Status != AcademicYearStatus.Active)
            throw new DomainException("Only an active academic year can be closed.");

        Status = AcademicYearStatus.Closed;
        IsCurrent = false;
    }
}

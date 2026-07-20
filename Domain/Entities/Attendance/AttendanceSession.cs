using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Attendance;

/// <summary>
/// Represents an attendance session for a specific class schedule on a given date, including the associated attendance records and lock status.
/// </summary>

public class AttendanceSession : TenantEntity, IAggregateRoot
{
    public Guid ClassScheduleId { get; private set; }
    public DateOnly Date        { get; private set; }
    public bool IsLocked        { get; private set; }

    private readonly List<AttendanceRecord> _records = new();
    public IReadOnlyCollection<AttendanceRecord> Records => _records.AsReadOnly();

    private AttendanceSession() { } // EF Core

    private AttendanceSession(Guid id, Guid schoolId, Guid classScheduleId, DateOnly date)
        : base(id, schoolId)
    {
        ClassScheduleId = classScheduleId;
        Date            = date;
        IsLocked        = false;
    }

    public static AttendanceSession Open(Guid schoolId, Guid classScheduleId, DateOnly date)
        => new(Guid.NewGuid(), schoolId, classScheduleId, date);

    public void RecordStudent(Guid studentEnrollmentId, AttendanceStatus status)
    {
        if (IsLocked)
            throw new DomainException("Cannot modify a locked attendance session.");

        var existing = _records.FirstOrDefault(r => r.StudentEnrollmentId == studentEnrollmentId);
        if (existing is not null)
            existing.UpdateStatus(status);
        else
            _records.Add(AttendanceRecord.Create(SchoolId, Id, studentEnrollmentId, status));
    }

    public void Lock()
    {
        if (_records.Count == 0)
            throw new DomainException("Cannot lock an attendance session with no records.");
        IsLocked = true;
    }
}

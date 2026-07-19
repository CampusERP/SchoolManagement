using Domain.Common;
using Domain.Enums;

namespace Domain.Entities.Attendance;

/// <summary>
/// Represents an attendance record for a student in a specific attendance session, including the student's enrollment, attendance status, and any associated notes.
/// </summary>

public class AttendanceRecord : AuditableEntity
{
    public Guid AttendanceSessionId  { get; private set; }
    public Guid StudentEnrollmentId  { get; private set; }
    public AttendanceStatus Status   { get; private set; }
    public string? Note              { get; private set; }

    private AttendanceRecord() { } // EF Core

    private AttendanceRecord(Guid id, Guid sessionId, Guid enrollmentId, AttendanceStatus status)
        : base(id)
    {
        AttendanceSessionId = sessionId;
        StudentEnrollmentId = enrollmentId;
        Status              = status;
    }

    internal static AttendanceRecord Create(Guid sessionId, Guid enrollmentId, AttendanceStatus status)
        => new(Guid.NewGuid(), sessionId, enrollmentId, status);

    internal void UpdateStatus(AttendanceStatus status) => Status = status;

    public void AddNote(string note) => Note = note;
}

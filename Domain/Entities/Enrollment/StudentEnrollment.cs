using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Enrollment;

/// <summary>
/// A student's placement in a ClassRoom for an AcademicYear. This is its
/// own aggregate root — deliberately NOT a child collection hanging off
/// Student — because its lifecycle (enroll, transfer, withdraw) is
/// independent of the student's identity. Attendance, grades, and exam
/// results should all key off StudentEnrollmentId, not raw StudentId, so
/// transfers/withdrawals don't corrupt historical records.
/// </summary>
public class StudentEnrollment : TenantEntity, IAggregateRoot
{
    public Guid StudentId { get; private set; }
    public Guid ClassRoomId { get; private set; }
    public Guid AcademicYearId { get; private set; }
    public DateTime EnrolledAtUtc { get; private set; }
    public EnrollmentStatus Status { get; private set; }

    private StudentEnrollment() { } // EF Core

    private StudentEnrollment(Guid id, Guid schoolId, Guid studentId, Guid classRoomId,
        Guid academicYearId) : base(id, schoolId)
    {
        StudentId = studentId;
        ClassRoomId = classRoomId;
        AcademicYearId = academicYearId;
        EnrolledAtUtc = DateTime.UtcNow;
        Status = EnrollmentStatus.Active;
    }

    public static StudentEnrollment Create(Guid schoolId, Guid studentId, Guid classRoomId,
        Guid academicYearId)
    {
        return new StudentEnrollment(Guid.NewGuid(), schoolId, studentId, classRoomId, academicYearId);
    }

    public void TransferTo(Guid newClassRoomId)
    {
        if (Status != EnrollmentStatus.Active)
            throw new DomainException("Only an active enrollment can be transferred.");

        ClassRoomId = newClassRoomId;
        Status = EnrollmentStatus.Transferred;
    }

    public void Withdraw()
    {
        if (Status == EnrollmentStatus.Withdrawn)
            throw new DomainException("Enrollment is already withdrawn.");

        Status = EnrollmentStatus.Withdrawn;
    }
}

using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Enrollment;

/// <summary>
/// Represents the enrollment of a student in a specific classroom for a given academic year.
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

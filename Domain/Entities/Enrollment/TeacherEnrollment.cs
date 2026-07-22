using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Enrollment;

public class TeacherEnrollment : TenantEntity, IAggregateRoot
{
    public Guid TeacherId { get; private set; }
    public Guid ClassRoomId { get; private set; }
    public Guid TermId { get; private set; }
    public DateTime EnrolledAtUtc { get; private set; }
    public EnrollmentStatus Status { get; private set; }

    private TeacherEnrollment() { }

    private TeacherEnrollment(Guid id, Guid schoolId, Guid teacherId, Guid classRoomId,
        Guid termId) : base(id, schoolId)
    {
        TeacherId = teacherId;
        ClassRoomId = classRoomId;
        TermId = termId;
        EnrolledAtUtc = DateTime.UtcNow;
        Status = EnrollmentStatus.Active;
    }

    public static TeacherEnrollment Create(Guid schoolId, Guid teacherId, Guid classRoomId,
        Guid termId)
    {
        return new TeacherEnrollment(Guid.NewGuid(), schoolId, teacherId, classRoomId, termId);
    }

    public void Withdraw()
    {
        if (Status == EnrollmentStatus.Withdrawn)
            throw new DomainException("Enrollment is already withdrawn.");

        Status = EnrollmentStatus.Withdrawn;
    }
}

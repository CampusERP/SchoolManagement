using Domain.Common;

namespace Domain.Entities.People;

public sealed class TeacherCampus : TenantEntity
{
    public Guid TeacherId { get; private set; }
    public Guid CampusId { get; private set; }

    private TeacherCampus() { }

    private TeacherCampus(Guid id, Guid schoolId, Guid teacherId, Guid campusId) : base(id, schoolId)
    {
        TeacherId = teacherId;
        CampusId = campusId;
    }

    public static TeacherCampus Create(Guid schoolId, Guid teacherId, Guid campusId) =>
        new(Guid.NewGuid(), schoolId, teacherId, campusId);
}

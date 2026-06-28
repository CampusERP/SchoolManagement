namespace Domain.Common;

public class TenantEntity : AuditableEntity
{
    public Guid SchoolId { get; protected set; }

    protected TenantEntity() : base() { }

    protected TenantEntity(Guid id, Guid schoolId) : base(id)
    {
        SchoolId = schoolId;
    }
}

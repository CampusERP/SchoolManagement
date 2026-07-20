using System.ComponentModel.DataAnnotations;

namespace Domain.Common;

public class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; }

    protected AuditableEntity() : base() { }
    protected AuditableEntity(Guid id) : base(id) { }
}
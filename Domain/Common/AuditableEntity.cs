using System.ComponentModel.DataAnnotations;

namespace Domain.Common;

public class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}
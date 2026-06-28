using Domain.Common;
using Domain.Enums;

namespace Domain.Entities.People;

/// <summary>
/// Join entity replacing
/// Supports divorced parents, step-parents, shared custody, and per-parent
/// visibility flags (e.g., one parent can see grades but not billing).
/// Not an aggregate root on its own — created/modified through Student or
/// Parent application services
/// carries real attributes beyond a plain FK pair.
/// </summary>
public class StudentGuardian : TenantEntity
{
    public Guid StudentId { get; private set; }
    public Guid ParentId { get; private set; }
    public GuardianRelationshipType RelationshipType { get; private set; }
    public bool IsPrimaryContact { get; private set; }
    public bool CanViewGrades { get; private set; }
    public bool CanViewBilling { get; private set; }

    private StudentGuardian() { } // EF Core

    private StudentGuardian(Guid id, Guid schoolId, Guid studentId, Guid parentId,
        GuardianRelationshipType relationshipType, bool isPrimaryContact,
        bool canViewGrades, bool canViewBilling) : base(id, schoolId)
    {
        StudentId = studentId;
        ParentId = parentId;
        RelationshipType = relationshipType;
        IsPrimaryContact = isPrimaryContact;
        CanViewGrades = canViewGrades;
        CanViewBilling = canViewBilling;
    }

    public static StudentGuardian Create(Guid schoolId, Guid studentId, Guid parentId,
        GuardianRelationshipType relationshipType, bool isPrimaryContact = false,
        bool canViewGrades = true, bool canViewBilling = false)
    {
        return new StudentGuardian(Guid.NewGuid(), schoolId, studentId, parentId,
            relationshipType, isPrimaryContact, canViewGrades, canViewBilling);
    }

    public void UpdatePermissions(bool canViewGrades, bool canViewBilling)
    {
        CanViewGrades = canViewGrades;
        CanViewBilling = canViewBilling;
    }
}

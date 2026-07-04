using Domain.Common;
using Domain.Enums;

namespace Domain.Entities.People;

/// <summary>
/// Represents the relationship between a student and their guardian (parent).
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

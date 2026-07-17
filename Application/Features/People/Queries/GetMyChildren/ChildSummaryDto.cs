namespace Application.Features.People.Queries.GetMyChildren;

public record ChildSummaryDto(Guid StudentId, Guid EnrollmentId, string StudentCode,
    string FirstName, string LastName, string RelationshipType,
    bool IsPrimaryContact, bool CanViewGrades, bool CanViewBilling,
    string? CurrentClass);

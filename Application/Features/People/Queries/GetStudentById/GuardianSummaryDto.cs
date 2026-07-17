namespace Application.Features.People.Queries.StudentDetails;

public record GuardianSummaryDto(Guid ParentId, string FirstName, string LastName,
    string RelationshipType, bool IsPrimaryContact);
namespace Application.Features.People.Queries.GetParents;

public record ParentListDto(Guid Id, string FirstName, string LastName,
    int ChildrenCount);

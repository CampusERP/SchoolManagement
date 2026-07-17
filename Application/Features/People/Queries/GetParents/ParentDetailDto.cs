using Application.Features.People.Queries.GetMyChildren;

namespace Application.Features.People.Queries.GetParents;

public record ParentDetailDto(Guid Id, string FirstName, string LastName,
    List<ChildSummaryDto> Children);

using MediatR;
using Application.Common.Models;
using Domain.Entities.Assignments;
using Application.Common.Interfaces.Repositories;

namespace Application.Features.Assignments.Commands.CreateAssignment;

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, Result<Guid>>
{
    private readonly IAssignmentRepository _assignments;

    public CreateAssignmentCommandHandler(IAssignmentRepository assignments) => _assignments = assignments;

    public async Task<Result<Guid>> Handle(CreateAssignmentCommand request, CancellationToken ct)
    {
        var assignment = Assignment.Create(
            request.SchoolId, request.TeachingAssignmentId,
            request.Title, request.Instructions, request.DueDate, request.MaxScore);

        await _assignments.AddAsync(assignment, ct);
        return Result.Success(assignment.Id);
    }
}

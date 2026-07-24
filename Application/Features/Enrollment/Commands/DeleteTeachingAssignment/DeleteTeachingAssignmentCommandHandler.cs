using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Enrollment.Commands.DeleteTeachingAssignment;

public class DeleteTeachingAssignmentCommandHandler : IRequestHandler<DeleteTeachingAssignmentCommand, Result<Guid>>
{
    private readonly ITeachingAssignmentRepository _assignments;

    public DeleteTeachingAssignmentCommandHandler(ITeachingAssignmentRepository assignments)
    {
        _assignments = assignments;
    }

    public async Task<Result<Guid>> Handle(DeleteTeachingAssignmentCommand request, CancellationToken ct)
    {
        var assignment = await _assignments.GetByIdAsync(request.TeachingAssignmentId, ct);
        if (assignment is null)
            return Result.Failure<Guid>("Teaching assignment not found.");

        if (assignment.SchoolId != request.SchoolId)
            return Result.Failure<Guid>("This assignment does not belong to this school.");

        await _assignments.DeleteAsync(assignment, ct);
        return Result.Success(assignment.Id);
    }
}

using MediatR;
using Application.Common.Exceptions;
using Application.Common.Models;
using Domain.Entities.Assignments;
using Domain.Exceptions;
using Application.Common.Interfaces.Repositories;

namespace Application.Features.Assignments.Commands.CreateAssignment;

public class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand, Result>
{
    private readonly IAssignmentRepository _assignments;

    public GradeSubmissionCommandHandler(IAssignmentRepository assignments) => _assignments = assignments;

    public async Task<Result> Handle(GradeSubmissionCommand request, CancellationToken ct)
    {
        var assignment = await _assignments.GetByIdAsync(request.AssignmentId, ct);
        if (assignment is null) throw new NotFoundException(nameof(Assignment), request.AssignmentId);

        var submission = assignment.Submissions.FirstOrDefault(s => s.Id == request.SubmissionId);
        if (submission is null) throw new NotFoundException(nameof(AssignmentSubmission), request.SubmissionId);

        try
        {
            submission.Mark(request.Grade, request.Feedback, assignment.MaxScore);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}

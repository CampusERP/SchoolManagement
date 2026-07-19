using MediatR;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Assignments;
using Domain.Entities.Documents;
using Domain.Exceptions;

namespace Application.Features.Assignments.Commands.CreateAssignment;

public class SubmitAssignmentCommandHandler : IRequestHandler<SubmitAssignmentCommand, Result<Guid>>
{
    private readonly IAssignmentRepository _assignments;
    private readonly IBlobStorageService   _blob;
    private readonly IDocumentRepository   _documents;
    private readonly ICurrentUserService   _currentUser;

    public SubmitAssignmentCommandHandler(
        IAssignmentRepository assignments,
        IBlobStorageService blob,
        IDocumentRepository documents,
        ICurrentUserService currentUser)
    {
        _assignments = assignments;
        _blob        = blob;
        _documents   = documents;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(SubmitAssignmentCommand request, CancellationToken ct)
    {
        var assignment = await _assignments.GetByIdAsync(request.AssignmentId, ct);
        if (assignment is null) throw new NotFoundException(nameof(Assignment), request.AssignmentId);

        AssignmentSubmission submission;
        try
        {
            submission = assignment.Submit(request.StudentEnrollmentId);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }

        // Upload any attached files.
        if (request.Files is { Count: > 0 })
        {
            foreach (var file in request.Files)
            {
                await using var content = file.Content;
                var blob = await _blob.UploadAsync(
                    content, file.FileName, file.ContentType, request.SchoolId, ct);

                var doc = Document.Create(
                    request.SchoolId, file.FileName, blob.BlobUrl,
                    file.ContentType, blob.FileSizeBytes, _currentUser.UserId!.Value);

                await _documents.AddAsync(doc, ct);

                submission.AttachDocument(doc.Id);
            }
        }

        return Result.Success(submission.Id);
    }
}

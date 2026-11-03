using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Documents.Queries;

public record GetDocumentQuery(Guid SchoolId, Guid DocumentId)
    : IRequest<Result<DownloadDocumentDto>>, ITenantScopedRequest;

public sealed class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, Result<DownloadDocumentDto>>
{
    private readonly IDocumentRepository _documents;
    private readonly IDocumentAttachmentRepository _attachments;
    private readonly IBlobStorageService _storage;

    public GetDocumentQueryHandler(IDocumentRepository documents, IDocumentAttachmentRepository attachments, IBlobStorageService storage)
    {
        _documents = documents;
        _attachments = attachments;
        _storage = storage;
    }

    public async Task<Result<DownloadDocumentDto>> Handle(GetDocumentQuery request, CancellationToken ct)
    {
        var document = await _documents.GetByIdAsync(request.DocumentId, ct);
        var attachment = await _attachments.GetByDocumentAsync(request.DocumentId, ct);
        if (document is null || attachment is null || document.SchoolId != request.SchoolId || attachment.SchoolId != request.SchoolId)
            return Result.Failure<DownloadDocumentDto>("Document not found.");
        var stream = await _storage.OpenReadAsync(document.BlobUrl, ct);
        return stream is null
            ? Result.Failure<DownloadDocumentDto>("The stored file could not be found.")
            : Result.Success(new DownloadDocumentDto(stream, document.FileName, document.ContentType));
    }
}

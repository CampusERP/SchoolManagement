using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.Features.Documents.Commands;

public record UploadDocumentCommand(
    Guid SchoolId,
    Guid OwnerId,
    DocumentOwnerType OwnerType,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Stream Content) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;

public record DeleteDocumentCommand(Guid SchoolId, Guid DocumentId)
    : ICommand, IBaseCommand, ITenantScopedRequest;

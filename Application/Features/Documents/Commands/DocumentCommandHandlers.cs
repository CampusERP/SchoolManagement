using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Documents;
using Domain.Enums;
using MediatR;

namespace Application.Features.Documents.Commands;

public sealed class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    private const long MaxFileSize = 25 * 1024 * 1024;
    private readonly IStudentRepository _students;
    private readonly ITeacherRepository _teachers;
    private readonly ISubjectBookRepository _books;
    private readonly IDocumentRepository _documents;
    private readonly IDocumentAttachmentRepository _attachments;
    private readonly IBlobStorageService _storage;
    private readonly ICurrentUserService _currentUser;

    public UploadDocumentCommandHandler(IStudentRepository students, ITeacherRepository teachers,
        ISubjectBookRepository books, IDocumentRepository documents, IDocumentAttachmentRepository attachments,
        IBlobStorageService storage, ICurrentUserService currentUser)
    {
        _students = students;
        _teachers = teachers;
        _books = books;
        _documents = documents;
        _attachments = attachments;
        _storage = storage;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        await using var content = request.Content;
        if (_currentUser.UserId is null) return Result.Failure<Guid>("An authenticated uploader is required.");
        if (request.FileSizeBytes <= 0 || request.FileSizeBytes > MaxFileSize)
            return Result.Failure<Guid>("File size must be between 1 byte and 25 MB.");

        var ownerExists = request.OwnerType switch
        {
            DocumentOwnerType.StudentProfile => await ExistsStudent(request.OwnerId, request.SchoolId, ct),
            DocumentOwnerType.TeacherProfile => await ExistsTeacher(request.OwnerId, request.SchoolId, ct),
            DocumentOwnerType.SubjectBook => await ExistsBook(request.OwnerId, request.SchoolId, ct),
            _ => false
        };
        if (!ownerExists) return Result.Failure<Guid>("Document owner was not found in this school.");

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var pdfOnly = request.OwnerType == DocumentOwnerType.SubjectBook;
        if (pdfOnly && (extension != ".pdf" || !string.Equals(request.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>("Book documents must be PDF files.");
        if (!pdfOnly && !IsAllowedProfileDocument(extension, request.ContentType))
            return Result.Failure<Guid>("Use a PDF, Word document, or JPEG/PNG image.");

        var upload = await _storage.UploadAsync(request.Content, request.FileName, request.ContentType, request.SchoolId, ct);
        var document = Document.Create(request.SchoolId, request.FileName, upload.BlobUrl,
            request.ContentType, upload.FileSizeBytes, _currentUser.UserId.Value);
        await _documents.AddAsync(document, ct);
        await _attachments.AddAsync(DocumentAttachment.Create(request.SchoolId, document.Id,
            request.OwnerId, request.OwnerType), ct);
        return Result.Success(document.Id);
    }

    private async Task<bool> ExistsStudent(Guid id, Guid schoolId, CancellationToken ct) =>
        (await _students.GetByIdAsync(id, ct))?.SchoolId == schoolId;
    private async Task<bool> ExistsTeacher(Guid id, Guid schoolId, CancellationToken ct) =>
        (await _teachers.GetByIdAsync(id, ct))?.SchoolId == schoolId;
    private async Task<bool> ExistsBook(Guid id, Guid schoolId, CancellationToken ct) =>
        (await _books.GetByIdAsync(id, ct))?.SchoolId == schoolId;

    private static bool IsAllowedProfileDocument(string extension, string contentType) =>
        (extension, contentType.ToLowerInvariant()) is
            (".pdf", "application/pdf") or
            (".doc", "application/msword") or
            (".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document") or
            (".jpg", "image/jpeg") or (".jpeg", "image/jpeg") or (".png", "image/png");
}

public sealed class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result>
{
    private readonly IDocumentRepository _documents;
    private readonly IDocumentAttachmentRepository _attachments;
    private readonly IBlobStorageService _storage;

    public DeleteDocumentCommandHandler(IDocumentRepository documents, IDocumentAttachmentRepository attachments, IBlobStorageService storage)
    {
        _documents = documents;
        _attachments = attachments;
        _storage = storage;
    }

    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken ct)
    {
        var document = await _documents.GetByIdAsync(request.DocumentId, ct);
        var attachment = await _attachments.GetByDocumentAsync(request.DocumentId, ct);
        if (document is null || attachment is null || document.SchoolId != request.SchoolId || attachment.SchoolId != request.SchoolId)
            return Result.Failure("Document not found.");

        await _storage.DeleteAsync(document.BlobUrl, ct);
        _attachments.Remove(attachment);
        _documents.Remove(document);
        return Result.Success();
    }
}

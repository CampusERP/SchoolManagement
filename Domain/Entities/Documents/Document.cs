using Domain.Common;

namespace Domain.Entities.Documents;

/// <summary>
/// Represents a document uploaded to the system, including its file name, blob URL, content type, file size, and the user who uploaded it.
/// </summary>

public class Document : TenantEntity, IAggregateRoot
{
    public string FileName          { get; private set; } = default!;
    public string BlobUrl           { get; private set; } = default!;
    public string ContentType       { get; private set; } = default!;
    public long   FileSizeBytes     { get; private set; }
    public Guid   UploadedByUserId  { get; private set; }

    private Document() { } // EF Core

    private Document(Guid id, Guid schoolId, string fileName, string blobUrl,
        string contentType, long fileSizeBytes, Guid uploadedByUserId)
        : base(id, schoolId)
    {
        FileName         = fileName;
        BlobUrl          = blobUrl;
        ContentType      = contentType;
        FileSizeBytes    = fileSizeBytes;
        UploadedByUserId = uploadedByUserId;
    }

    public static Document Create(Guid schoolId, string fileName, string blobUrl,
        string contentType, long fileSizeBytes, Guid uploadedByUserId)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name required.");
        if (string.IsNullOrWhiteSpace(blobUrl))  throw new ArgumentException("Blob URL required.");

        return new Document(Guid.NewGuid(), schoolId, fileName, blobUrl,
            contentType, fileSizeBytes, uploadedByUserId);
    }
}

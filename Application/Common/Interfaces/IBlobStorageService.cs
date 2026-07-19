namespace Application.Common.Interfaces;

public record BlobUploadResult(string BlobUrl, string StoredFileName, long FileSizeBytes);

public interface IBlobStorageService
{
    Task<BlobUploadResult> UploadAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        Guid schoolId,
        CancellationToken ct = default);

    Task DeleteAsync(string blobUrl, CancellationToken ct = default);
}

using Microsoft.Extensions.Configuration;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Services;

/// <summary>
/// Stores files on local disk under wwwroot/uploads/{schoolId}/.
/// Used in development. Swap for AzureBlobStorageService in production
/// by changing the DI registration — Application code is untouched.
/// </summary>
public class LocalBlobStorageService : IBlobStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalBlobStorageService(IConfiguration config)
    {
        _basePath = config["BlobStorage:LocalBasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl  = config["BlobStorage:LocalBaseUrl"]  ?? "http://localhost:5000/uploads";

        Directory.CreateDirectory(_basePath);
    }

    public async Task<BlobUploadResult> UploadAsync(
        Stream fileStream, string originalFileName, string contentType,
        Guid schoolId, CancellationToken ct = default)
    {
        var schoolFolder = Path.Combine(_basePath, schoolId.ToString());
        Directory.CreateDirectory(schoolFolder);

        var ext           = Path.GetExtension(originalFileName);
        var storedName    = $"{Guid.NewGuid()}{ext}";
        var fullPath      = Path.Combine(schoolFolder, storedName);
        var blobUrl       = $"{_baseUrl}/{schoolId}/{storedName}";

        await using var fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs, ct);

        return new BlobUploadResult(blobUrl, storedName, fs.Length);
    }

    public Task DeleteAsync(string blobUrl, CancellationToken ct = default)
    {
        // Reverse the URL back to a local path and delete.
        var relativePath = blobUrl.Replace(_baseUrl, "").TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath     = Path.Combine(_basePath, relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}

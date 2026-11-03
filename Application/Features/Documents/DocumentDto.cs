namespace Application.Features.Documents;

public record DocumentDto(Guid Id, string FileName, string ContentType, long FileSizeBytes, DateTime CreatedAtUtc);
public record DownloadDocumentDto(Stream Content, string FileName, string ContentType);

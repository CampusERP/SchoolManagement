namespace Application.Common.Models;

/// <summary>
/// A file supplied to an application command. This deliberately avoids exposing
/// ASP.NET Core's <c>IFormFile</c> outside the API layer.
/// </summary>
public sealed record SubmissionFile(
    Stream Content,
    string FileName,
    string ContentType);

using Application.Common.Interfaces.Services;
using Application.Features.Subjects.Queries.GetGradeCurriculum;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public sealed class SubjectReadService : ISubjectReadService
{
    private readonly ApplicationDbContext _db;
    public SubjectReadService(ApplicationDbContext db) => _db = db;

    public async Task<List<CurriculumSubjectDto>> GetGradeCurriculumAsync(Guid schoolId, Guid gradeLevelId, CancellationToken ct = default)
    {
        var subjects = await (
            from curriculum in _db.CurriculumSubjects.AsNoTracking()
            where curriculum.SchoolId == schoolId && curriculum.GradeLevelId == gradeLevelId
            join subject in _db.Subjects.AsNoTracking() on curriculum.SubjectId equals subject.Id
            orderby subject.Name
            select new { curriculum.Id, curriculum.SubjectId, subject.Code, subject.Name, subject.Description, subject.SchoolId }
        ).ToListAsync(ct);

        var curriculumIds = subjects.Select(subject => subject.Id).ToArray();
        var books = await _db.SubjectBooks.AsNoTracking()
            .Where(book => book.SchoolId == schoolId && curriculumIds.Contains(book.CurriculumSubjectId))
            .OrderBy(book => book.Title)
            .Select(book => new { book.Id, book.CurriculumSubjectId, book.Title, book.Author, book.Publisher, book.Isbn, book.Edition, book.Notes })
            .ToListAsync(ct);

        var bookIds = books.Select(book => book.Id).ToArray();
        var bookDocuments = await (from attachment in _db.DocumentAttachments.AsNoTracking()
                                   where attachment.OwnerType == DocumentOwnerType.SubjectBook && bookIds.Contains(attachment.OwnerId)
                                   join document in _db.Documents.AsNoTracking() on attachment.DocumentId equals document.Id
                                   orderby document.CreatedAtUtc descending
                                   select new { attachment.OwnerId, Dto = new Application.Features.Documents.DocumentDto(
                                       document.Id, document.FileName, document.ContentType, document.FileSizeBytes, document.CreatedAtUtc) })
            .ToListAsync(ct);

        return subjects.Select(subject => new CurriculumSubjectDto(
            subject.Id,
            subject.SubjectId,
            subject.Code,
            subject.Name,
            subject.Description,
            subject.SchoolId == schoolId,
            books.Where(book => book.CurriculumSubjectId == subject.Id)
                .Select(book => new CurriculumBookDto(book.Id, book.Title, book.Author, book.Publisher, book.Isbn, book.Edition, book.Notes,
                    bookDocuments.Where(document => document.OwnerId == book.Id).Select(document => document.Dto).ToList()))
                .ToList())).ToList();
    }
}

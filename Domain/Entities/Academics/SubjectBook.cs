using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>A school-specific book assigned to a subject in a grade's curriculum.</summary>
public class SubjectBook : TenantEntity
{
    public Guid CurriculumSubjectId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Author { get; private set; }
    public string? Publisher { get; private set; }
    public string? Isbn { get; private set; }
    public string? Edition { get; private set; }
    public string? Notes { get; private set; }

    private SubjectBook() { }

    private SubjectBook(Guid id, Guid schoolId, Guid curriculumSubjectId, string title,
        string? author, string? publisher, string? isbn, string? edition, string? notes)
        : base(id, schoolId)
    {
        CurriculumSubjectId = curriculumSubjectId;
        Title = title;
        Author = author;
        Publisher = publisher;
        Isbn = isbn;
        Edition = edition;
        Notes = notes;
    }

    public static SubjectBook Create(Guid schoolId, Guid curriculumSubjectId, string title,
        string? author, string? publisher, string? isbn, string? edition, string? notes)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Book title is required.", nameof(title));

        return new SubjectBook(Guid.NewGuid(), schoolId, curriculumSubjectId, title.Trim(),
            Normalize(author), Normalize(publisher), Normalize(isbn), Normalize(edition), Normalize(notes));
    }

    public void Update(string title, string? author, string? publisher, string? isbn, string? edition, string? notes)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Book title is required.", nameof(title));

        Title = title.Trim();
        Author = Normalize(author);
        Publisher = Normalize(publisher);
        Isbn = Normalize(isbn);
        Edition = Normalize(edition);
        Notes = Normalize(notes);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

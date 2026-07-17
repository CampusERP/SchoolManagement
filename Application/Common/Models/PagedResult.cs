namespace Application.Common.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    public static PagedResult<T> Empty(int page, int pageSize)
        => new(Array.Empty<T>(), 0, page, pageSize);
}


public record PaginationParams(int Page = 1, int PageSize = 20)
{
    public int Page { get; } = Math.Max(1, Page);
    public int PageSize { get; } = Math.Clamp(PageSize, 1, 100);
    public int Skip => (Page - 1) * PageSize;
}
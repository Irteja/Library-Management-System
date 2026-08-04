namespace LibraryManagementSystem.Application.Common.Models;

public class CursorPaginatedList<T>
{
    public List<T> Items { get; }
    public string? NextCursor { get; }
    public bool HasNextPage { get; }

    public CursorPaginatedList(List<T> items, string? nextCursor, bool hasNextPage)
    {
        Items = items;
        NextCursor = nextCursor;
        HasNextPage = hasNextPage;
    }
}

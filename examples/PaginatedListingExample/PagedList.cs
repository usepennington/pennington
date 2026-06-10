namespace PaginatedListingExample;

/// <summary>One page of a longer list, plus the metadata the <c>Pagination</c> component needs.</summary>
/// <typeparam name="T">Item type held in <see cref="Items"/>.</typeparam>
/// <param name="Items">The items on this page.</param>
/// <param name="Page">1-based index of this page.</param>
/// <param name="PageSize">Maximum items per page.</param>
/// <param name="TotalItems">Total item count across every page.</param>
public sealed record PagedList<T>(
    IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems)
{
    /// <summary>Total number of pages, never less than one.</summary>
    public int TotalPages => TotalItems <= 0 || PageSize <= 0
        ? 1
        : (int)Math.Ceiling(TotalItems / (double)PageSize);

    /// <summary>True when a previous page exists.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>True when a next page exists.</summary>
    public bool HasNext => Page < TotalPages;
}

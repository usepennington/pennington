namespace Pennington.BlogSite.Services;

/// <summary>
/// A single page of items with the bookkeeping the <c>Pagination</c> component needs to render
/// prev/next controls and numbered page links.
/// </summary>
/// <typeparam name="T">Item type contained in the page.</typeparam>
/// <param name="Items">Items in this page slice.</param>
/// <param name="Page">1-based page index this slice represents.</param>
/// <param name="PageSize">Items per page.</param>
/// <param name="TotalItems">Total item count across all pages.</param>
public sealed record PagedList<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    /// <summary>Total page count. At least <c>1</c> even when <see cref="TotalItems"/> is zero.</summary>
    public int TotalPages => TotalItems <= 0 || PageSize <= 0
        ? 1
        : (int)Math.Ceiling(TotalItems / (double)PageSize);

    /// <summary>True when a page exists before <see cref="Page"/>.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>True when a page exists after <see cref="Page"/>.</summary>
    public bool HasNext => Page < TotalPages;
}

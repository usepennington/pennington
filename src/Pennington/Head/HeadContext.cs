namespace Pennington.Head;

using Content;
using Microsoft.AspNetCore.Http;

/// <summary>Per-request inputs handed to every <see cref="IHeadContributor"/>.</summary>
public sealed class HeadContext
{
    /// <summary>The active HTTP request.</summary>
    public required HttpContext HttpContext { get; init; }

    /// <summary>
    /// Request path with the locale segment reattached (<c>PathBase + Path</c>) — the same key
    /// <see cref="ContentRecordRegistry"/> joins on (after trimming slashes).
    /// </summary>
    public required string FullPath { get; init; }

    /// <summary>The content record resolved for this request, or <c>null</c> for endpoint/404 pages.</summary>
    public ContentRecord? Record { get; init; }
}

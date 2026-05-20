namespace Pennington.LlmsTxt;

/// <summary>
/// Declares that all leaves under <see cref="RoutePrefix"/> should be split out of the
/// main <c>/llms.txt</c> into a dedicated <c>{RoutePrefix}llms.txt</c>, leaving the front
/// door with a single see-also pointer line.
/// </summary>
public sealed record LlmsSubtree
{
    /// <summary>Initializes a subtree, normalizing <paramref name="routePrefix"/> to <c>/foo/bar/</c> form.</summary>
    public LlmsSubtree(string routePrefix, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(routePrefix))
        {
            throw new ArgumentException("RoutePrefix must be non-empty.", nameof(routePrefix));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title must be non-empty.", nameof(title));
        }

        var trimmed = routePrefix.Trim('/');
        RoutePrefix = trimmed.Length == 0 ? "/" : "/" + trimmed + "/";
        Title = title;
        Description = description ?? "";
    }

    /// <summary>URL prefix in canonical <c>/foo/bar/</c> form (always leading and trailing slash).</summary>
    public string RoutePrefix { get; }

    /// <summary>Header rendered at the top of the subtree's <c>llms.txt</c>.</summary>
    public string Title { get; }

    /// <summary>Short blurb rendered after the title and as the see-also pointer description.</summary>
    public string Description { get; }
}
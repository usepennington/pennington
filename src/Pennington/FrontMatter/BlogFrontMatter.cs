namespace Pennington.FrontMatter;

/// <summary>
/// Core-library front matter for blog posts on bare
/// <see cref="Infrastructure.PenningtonExtensions.AddPennington"/> hosts.
/// Carries <see cref="Date"/>, <see cref="Author"/>, and <see cref="Series"/> alongside the
/// <see cref="IFrontMatter"/> defaults, and implements <see cref="IFrontMatter"/> and
/// <see cref="ITaggable"/>. Not the record bound by <c>AddBlogSite</c> — see
/// <c>BlogSiteFrontMatter</c> in the <c>Pennington.BlogSite</c> package for that.
/// </summary>
public record BlogFrontMatter : IFrontMatter, ITaggable
{
    /// <inheritdoc/>
    public string Title { get; init; } = "";

    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <inheritdoc/>
    public bool IsDraft { get; init; }

    /// <inheritdoc/>
    public string[] Tags { get; init; } = [];

    /// <inheritdoc/>
    public DateTime? Date { get; init; }

    /// <summary>Optional author name rendered in post bylines and feeds.</summary>
    public string? Author { get; init; }

    /// <summary>Optional series identifier used to group related posts together.</summary>
    public string? Series { get; init; }

    /// <inheritdoc/>
    public string? Uid { get; init; }

    /// <inheritdoc/>
    public bool Search { get; init; } = true;

    /// <inheritdoc/>
    public bool Llms { get; init; } = true;

    /// <inheritdoc/>
    public bool SearchOnly { get; init; }
}
namespace Pennington.FrontMatter;

/// <summary>
/// Core-library front matter for documentation pages on bare
/// <see cref="Infrastructure.PenningtonExtensions.AddPennington"/> hosts.
/// Implements <see cref="IFrontMatter"/>, <see cref="ITaggable"/>, <see cref="ISectionable"/>,
/// and <see cref="IOrderable"/> — the default capability shape for doc content without the
/// DocSite template. Hosts using <c>AddDocSite</c> bind
/// <c>DocSiteFrontMatter</c> from the <c>Pennington.DocSite</c> package instead.
/// </summary>
public record DocFrontMatter : IFrontMatter, ITaggable,
    ISectionable, IOrderable
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
    public string? SectionLabel { get; init; }

    /// <inheritdoc/>
    public string? Uid { get; init; }

    /// <inheritdoc/>
    public int Order { get; init; } = int.MaxValue;

    /// <inheritdoc/>
    public bool Search { get; init; } = true;

    /// <inheritdoc/>
    public bool Llms { get; init; } = true;

    /// <inheritdoc/>
    public bool SearchOnly { get; init; }
}
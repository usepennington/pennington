namespace CustomFrontMatterRazorPageExample;

using Pennington.FrontMatter;

/// <summary>
/// Custom front-matter record adding a per-page <see cref="Namespace"/> and
/// <see cref="Stability"/> on top of the built-in keys. A bare host registers
/// it against a markdown source so symbol pages deserialize the extra YAML keys.
/// </summary>
public record ApiFrontMatter : IFrontMatter, ITaggable, ISectionable, IOrderable, IRedirectable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public int Order { get; init; } = int.MaxValue;
    public string? RedirectUrl { get; init; }
    public string? SectionLabel { get; init; }
    public string? Uid { get; init; }
    public bool Search { get; init; } = true;
    public bool Llms { get; init; } = true;

    /// <summary>API namespace (e.g. <c>Pennington.Highlighting</c>).</summary>
    public string? Namespace { get; init; }

    /// <summary>Stability classification — <c>stable</c>, <c>preview</c>, or <c>experimental</c>.</summary>
    public string Stability { get; init; } = "stable";
}

namespace Pennington.Head;

/// <summary>
/// Identity used for head deduplication. Two tags with the same key collapse to one — the first
/// contributor to add at a key wins (contributors run lowest-<see cref="IHeadContributor.Order"/>
/// first, so page-level beats site-level). Repeatable tags (hreflang, JSON-LD, preloads) opt out
/// by being added with no key via <see cref="HeadBuilder.AddRepeatable"/>.
/// </summary>
/// <param name="Value">Stable string identity (e.g. <c>title</c>, <c>meta:prop:og:image</c>, <c>link:rel:canonical</c>).</param>
public readonly record struct HeadTagKey(string Value)
{
    /// <summary>The singleton document title.</summary>
    public static readonly HeadTagKey Title = new("title");

    /// <summary>Builds the key for a named meta tag (e.g. <c>meta:name:description</c>).</summary>
    public static HeadTagKey MetaName(string name) => new($"meta:name:{name}");

    /// <summary>Builds the key for an OpenGraph/property meta tag (e.g. <c>meta:prop:og:image</c>).</summary>
    public static HeadTagKey MetaProperty(string property) => new($"meta:prop:{property}");

    /// <summary>Builds the key for a singleton link rel (e.g. <c>link:rel:canonical</c>).</summary>
    public static HeadTagKey LinkRel(string rel) => new($"link:rel:{rel}");
}

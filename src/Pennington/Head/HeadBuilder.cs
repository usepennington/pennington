namespace Pennington.Head;

/// <summary>
/// Collects <see cref="HeadTag"/>s from every contributor. Keyed tags deduplicate by
/// <see cref="HeadTagKey"/> (first add at a key wins; later same-key adds are dropped) while
/// preserving first-seen order; keyless tags (added via <see cref="AddRepeatable"/>) always append.
/// </summary>
public sealed class HeadBuilder
{
    private readonly Dictionary<string, HeadTag> _keyed = new(StringComparer.Ordinal);
    private readonly List<string> _keyOrder = [];
    private readonly List<HeadTag> _keyless = [];

    /// <summary>Adds a tag under an explicit dedup key; the first add at a key wins.</summary>
    public HeadBuilder Add(HeadTagKey key, HeadTag tag)
    {
        if (_keyed.TryAdd(key.Value, tag))
        {
            _keyOrder.Add(key.Value);
        }

        return this;
    }

    /// <summary>Adds a repeatable tag (hreflang, JSON-LD, preload) with no deduplication.</summary>
    public HeadBuilder AddRepeatable(HeadTag tag)
    {
        _keyless.Add(tag);
        return this;
    }

    /// <summary>Sets the document title (deduplicated to one).</summary>
    public HeadBuilder Title(string text) => Add(HeadTagKey.Title, new HeadTag(new TitleTag(text)));

    /// <summary>Sets a named meta tag, deduplicated on its <paramref name="name"/>.</summary>
    public HeadBuilder Meta(string name, string content)
        => Add(HeadTagKey.MetaName(name), new HeadTag(new MetaNameTag(name, content)));

    /// <summary>Sets an OpenGraph/property meta tag, deduplicated on its <paramref name="property"/>.</summary>
    public HeadBuilder Property(string property, string content)
        => Add(HeadTagKey.MetaProperty(property), new HeadTag(new MetaPropertyTag(property, content)));

    /// <summary>Sets a singleton link (e.g. canonical), deduplicated on its <paramref name="rel"/>.</summary>
    public HeadBuilder Link(string rel, string href)
        => Add(HeadTagKey.LinkRel(rel), new HeadTag(new LinkTag(rel, href)));

    /// <summary>The composed entries: keyed tags first (first-seen order), then keyless tags (append order).</summary>
    public IReadOnlyList<HeadEntry> Build()
    {
        var result = new List<HeadEntry>(_keyOrder.Count + _keyless.Count);
        foreach (var key in _keyOrder)
        {
            result.Add(new HeadEntry(new HeadTagKey(key), _keyed[key]));
        }

        foreach (var tag in _keyless)
        {
            result.Add(new HeadEntry(null, tag));
        }

        return result;
    }
}

/// <summary>A composed head tag paired with its dedup key (<c>null</c> for repeatable tags).</summary>
/// <param name="Key">Dedup key, or <c>null</c> when the tag is repeatable.</param>
/// <param name="Tag">The tag to emit.</param>
public readonly record struct HeadEntry(HeadTagKey? Key, HeadTag Tag);

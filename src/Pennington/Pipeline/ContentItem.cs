namespace Pennington.Pipeline;

using FrontMatter;
using Routing;

// Case types — each is a standalone record

/// <summary>A content item discovered by a content service but not yet run through the pipeline.</summary>
/// <param name="Route">Canonical route for the item.</param>
/// <param name="Source">Origin describing how the item's content is produced.</param>
public record DiscoveredItem(ContentRoute Route, ContentSource Source)
{
    /// <summary>Front matter the discovering service already parsed, when its source carried any; null for sources whose metadata is not known until parse or render time.</summary>
    public IFrontMatter? Metadata { get; init; }
}

/// <summary>A content item whose front matter and raw markdown body have been parsed.</summary>
/// <param name="Route">Canonical route for the item.</param>
/// <param name="Metadata">Parsed front matter metadata.</param>
/// <param name="RawMarkdown">Markdown body text, with front matter stripped.</param>
public record ParsedItem(ContentRoute Route, IFrontMatter Metadata, string RawMarkdown);

/// <summary>A content item whose body has been rendered to HTML.</summary>
/// <param name="Route">Canonical route for the item.</param>
/// <param name="Metadata">Parsed front matter metadata.</param>
/// <param name="Content">Rendered content and extracted page data.</param>
public record RenderedItem(ContentRoute Route, IFrontMatter Metadata, RenderedContent Content);

/// <summary>A content item that failed during parsing or rendering.</summary>
/// <param name="Route">Canonical route for the item that failed.</param>
/// <param name="Error">Error describing the failure.</param>
public record FailedItem(ContentRoute Route, ContentError Error);

/// <summary>Union of all content item states flowing through the pipeline.</summary>
// The union — compiler enforces exhaustive matching over exactly these four types.
// The #else branch is a transitional net10.0 shim while C# 15 unions require net11.0.
#if NET11_0_OR_GREATER
public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem)
{
    /// <summary>The route for the current item regardless of state.</summary>
    public ContentRoute Route => this switch
    {
        DiscoveredItem d => d.Route,
        ParsedItem p => p.Route,
        RenderedItem r => r.Route,
        FailedItem f => f.Route,
        null => throw new InvalidOperationException("Uninitialized ContentItem")
    };
}
#else
[System.Runtime.CompilerServices.Union]
public readonly struct ContentItem : System.Runtime.CompilerServices.IUnion
{
    /// <summary>Wrapped case instance; inspect via pattern matching on the case types.</summary>
    public object? Value { get; }
    /// <summary>Wraps a <see cref="DiscoveredItem"/>.</summary>
    public ContentItem(DiscoveredItem value) { Value = value; }
    /// <summary>Wraps a <see cref="ParsedItem"/>.</summary>
    public ContentItem(ParsedItem value) { Value = value; }
    /// <summary>Wraps a <see cref="RenderedItem"/>.</summary>
    public ContentItem(RenderedItem value) { Value = value; }
    /// <summary>Wraps a <see cref="FailedItem"/>.</summary>
    public ContentItem(FailedItem value) { Value = value; }
    /// <summary>Implicit conversion from <see cref="DiscoveredItem"/>.</summary>
    public static implicit operator ContentItem(DiscoveredItem value) => new(value);
    /// <summary>Implicit conversion from <see cref="ParsedItem"/>.</summary>
    public static implicit operator ContentItem(ParsedItem value) => new(value);
    /// <summary>Implicit conversion from <see cref="RenderedItem"/>.</summary>
    public static implicit operator ContentItem(RenderedItem value) => new(value);
    /// <summary>Implicit conversion from <see cref="FailedItem"/>.</summary>
    public static implicit operator ContentItem(FailedItem value) => new(value);

    /// <summary>The route for the current item regardless of state.</summary>
    public ContentRoute Route => Value switch
    {
        DiscoveredItem d => d.Route,
        ParsedItem p => p.Route,
        RenderedItem r => r.Route,
        FailedItem f => f.Route,
        _ => throw new InvalidOperationException("Uninitialized ContentItem")
    };
}
#endif
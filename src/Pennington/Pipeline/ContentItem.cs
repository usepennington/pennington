namespace Pennington.Pipeline;

using FrontMatter;
using Routing;

// Case types — each is a standalone record

/// <summary>A content item discovered by a content service but not yet parsed.</summary>
/// <param name="Route">Canonical route for the item.</param>
/// <param name="Source">Origin describing how the item's content is produced.</param>
public record DiscoveredItem(ContentRoute Route, ContentSource Source);

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
// The union — compiler enforces exhaustive matching over exactly these four types
public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem)
{
    /// <summary>The route for the current item regardless of state.</summary>
    // Every case carries a Route — expose it on the union to avoid pattern matching at every call site
    public ContentRoute Route => this switch
    {
        DiscoveredItem d => d.Route,
        ParsedItem p => p.Route,
        RenderedItem r => r.Route,
        FailedItem f => f.Route,
        null => throw new InvalidOperationException("Uninitialized ContentItem")
    };
}

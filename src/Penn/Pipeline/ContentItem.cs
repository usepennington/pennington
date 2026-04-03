namespace Penn.Pipeline;

using Penn.FrontMatter;
using Penn.Routing;

// Case types — each is a standalone record
public record DiscoveredItem(ContentRoute Route, ContentSource Source);
public record ParsedItem(ContentRoute Route, IFrontMatter Metadata, string RawMarkdown);
public record RenderedItem(ContentRoute Route, IFrontMatter Metadata, RenderedContent Content);
public record FailedItem(ContentRoute Route, ContentError Error);

// The union — compiler enforces exhaustive matching over exactly these four types
public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem)
{
    // Every case carries a Route — expose it on the union to avoid pattern matching at every call site
    public ContentRoute Route => this switch
    {
        DiscoveredItem d => d.Route,
        ParsedItem p     => p.Route,
        RenderedItem r   => r.Route,
        FailedItem f     => f.Route,
        _ => throw new InvalidOperationException("Uninitialized ContentItem")
    };
}

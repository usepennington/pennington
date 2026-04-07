---
title: "Content Processing Pipeline"
description: "How Penn transforms content from discovery to rendered HTML using C# 15 union types and a streaming pipeline"
uid: "penn.under-the-hood.content-processing-pipeline"
order: 3010
---

Penn processes content through a four-stage streaming pipeline. Markdown files, Razor pages, redirects, and programmatic content all enter the same pipeline and exit as rendered HTML or structured error reports. The pipeline uses C# 15 union types to represent content items at each stage, which means the compiler enforces exhaustive handling at every step. If you add a new stage or a new failure mode, every `switch` expression that touches a `ContentItem` must be updated before the code compiles.

## The ContentItem Union

Every piece of content flowing through the pipeline is a `ContentItem` -- a union type with exactly four cases:

```csharp
// The four stages of content
public record DiscoveredItem(ContentRoute Route, ContentSource Source);
public record ParsedItem(ContentRoute Route, IFrontMatter Metadata, string RawMarkdown);
public record RenderedItem(ContentRoute Route, IFrontMatter Metadata, RenderedContent Content);
public record FailedItem(ContentRoute Route, ContentError Error);

// The union -- compiler enforces exhaustive matching
public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem);
```

`DiscoveredItem` pairs a route with a content source. `ParsedItem` carries extracted front matter and the raw markdown body. `RenderedItem` holds the final HTML along with outline entries, tags, cross-references, and search metadata. `FailedItem` captures what went wrong and where.

The union enforces exhaustive matching. Write a `switch` over `ContentItem` and omit a case -- the compiler rejects it. This is a compile error, not a warning, not a suggestion.

Construction wraps a case record in the union:

```csharp
var item = new ContentItem(new DiscoveredItem(route, source));
```

Pattern matching is the primary consumption model:

```csharp
var message = item switch
{
    DiscoveredItem d => $"Found: {d.Route}",
    ParsedItem p     => $"Parsed: {p.Route} with {p.Metadata.Title}",
    RenderedItem r   => $"Rendered: {r.Route}",
    FailedItem f     => $"Failed: {f.Route} - {f.Error.Message}",
};
```

Every case carries a `ContentRoute`, and the union exposes a shared `Route` property so callers can identify a content item's location without pattern matching:

```csharp
public ContentRoute Route => this switch
{
    DiscoveredItem d => d.Route,
    ParsedItem p     => p.Route,
    RenderedItem r   => r.Route,
    FailedItem f     => f.Route,
};
```

## ContentSource

The `ContentSource` union describes where a piece of content comes from:

```csharp
public record MarkdownFileSource(FilePath Path);
public record RazorPageSource(string ComponentType);
public record RedirectSource(UrlPath TargetUrl);
public record ProgrammaticSource(IProgrammaticContentGenerator Generator);

public union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource);
```

Four cases:

- **`MarkdownFileSource`** -- a markdown file on disk, identified by a `FilePath`.
- **`RazorPageSource`** -- a Blazor component, identified by its type name.
- **`RedirectSource`** -- not real content, just a URL to redirect to.
- **`ProgrammaticSource`** -- content generated at runtime by an `IProgrammaticContentGenerator` implementation.

The pipeline does not need to understand the details of each source. It delegates to the appropriate `IContentParser` implementation, which knows how to read a `MarkdownFileSource` differently from a `ProgrammaticSource`. The union ensures that new source types cannot be added without updating every parser that handles content sources.

## The Four Pipeline Stages

The pipeline is a linear chain of `IAsyncEnumerable<ContentItem>` streams:

```
                    IContentService[]
                          |
                    DiscoverAsync()
                          |
                  +-------v--------+
                  | DiscoveredItem |----+
                  | DiscoveredItem |    |
                  | DiscoveredItem |    |  FailedItems pass
                  +-------+--------+    |  through unchanged
                          |             |
                    ParseAsync()        |
                          |             |
                  +-------v--------+    |
                  | ParsedItem     |<---+
                  | FailedItem     |
                  | ParsedItem     |
                  +-------+--------+
                          |
                   RenderAsync()
                          |
                  +-------v--------+
                  | RenderedItem   |
                  | FailedItem     |
                  | RenderedItem   |
                  +-------+--------+
                          |
                  GenerateAsync()
                          |
                  +-------v--------+
                  | BuildReport    |
                  +----------------+
```

Each stage receives the full output of the previous stage, transforms the items it understands, and passes everything else through unchanged.

### Stage 1: Discover

Content services -- implementations of `IContentService` -- yield `DiscoveredItem` records. Each pairs a `ContentRoute` with a `ContentSource`. The pipeline iterates all registered services and yields their discoveries as a flat stream:

```csharp
public async IAsyncEnumerable<ContentItem> DiscoverAsync()
{
    foreach (var service in _services)
    {
        await foreach (var discovered in service.DiscoverAsync())
        {
            yield return discovered;
        }
    }
}
```

A `MarkdownContentService` scans a directory for `.md` files. A `RazorPageContentService` reflects over Blazor components with `@page` directives. Custom services can discover content from databases, APIs, or generated data. All of them produce the same `DiscoveredItem` records.

Discovery is optimistic. No failures happen here -- the pipeline has not tried to read or parse anything yet.

### Stage 2: Parse

`ParseAsync` receives the stream of items and transforms `DiscoveredItem` records into `ParsedItem` records. For markdown files, this means reading the file, extracting YAML front matter, and separating the markdown body. The parser produces:

```csharp
new ContentItem(new ParsedItem(item.Route, metadata, result.Body))
```

If anything goes wrong -- file not found, malformed YAML, an unexpected encoding -- the item becomes a `FailedItem`:

```csharp
new ContentItem(new FailedItem(item.Route,
    new ContentError($"Parse failed: {ex.Message}", ex)))
```

Items that are already `FailedItem` pass through unchanged. Items that are already `ParsedItem` or `RenderedItem` (from a previous pipeline run or a custom source) also pass through:

```csharp
if (item is FailedItem)
{
    yield return item;
    continue;
}
```

### Stage 3: Render

`RenderAsync` transforms `ParsedItem` records into `RenderedItem` records. For markdown, `MarkdownContentRenderer` runs the raw body through the full Markdig pipeline, which handles syntax highlighting, tabbed code blocks, alert blocks, heading ID generation, and the rest of the Markdig extensions.

The output is a `RenderedContent` record:

```csharp
public record RenderedContent(
    string Html,
    OutlineEntry[] Outline,
    ImmutableList<Tag> Tags,
    ImmutableList<CrossReference> CrossReferences,
    SearchIndexDocument? SearchDocument,
    SocialMetadata? Social
);
```

This is not just HTML. The renderer also extracts the heading outline (for table-of-contents navigation), collects tags, resolves cross-references, builds a search index document, and gathers social metadata -- all in one pass.

Failures follow the same pattern as Parse. Exceptions are caught at the stage boundary and wrapped in `FailedItem`. No exception escapes the stage.

### Stage 4: Generate

`GenerateAsync` consumes the entire stream and produces a `BuildReport`. This is where final decisions happen:

```csharp
switch (item)
{
    case RenderedItem rendered:
        if (rendered.Metadata is IDraftable { IsDraft: true })
            reportBuilder.AddSkippedPage(rendered.Route);
        else
            reportBuilder.AddGeneratedPage(rendered.Route);
        break;

    case FailedItem failed:
        reportBuilder.AddError(failed.Route, failed.Error.Message, failed.Error.Exception);
        break;

    default:
        reportBuilder.AddWarning(item.Route, "Item did not complete pipeline");
        break;
}
```

Draft filtering happens here, not at an earlier stage. Draft items travel through the entire pipeline -- discovered, parsed, rendered -- and are only excluded at the output stage. This means you can preview drafts during development while ensuring they never appear in production builds.

The Generate stage also checks rendered HTML for internal links missing trailing slashes and adds those as warnings to the build report. Items that are still `DiscoveredItem` or `ParsedItem` at this point trigger warnings -- something prevented them from reaching the `RenderedItem` state.

## The RunAsync Convenience Method

For the common case of running the full pipeline end-to-end, `ContentPipeline.RunAsync` chains all four stages:

```csharp
public async Task<BuildReport> RunAsync(OutputOptions options)
{
    var discovered = DiscoverAsync();
    var parsed = ParseAsync(discovered);
    var rendered = RenderAsync(parsed);
    return await GenerateAsync(rendered, options);
}
```

Because every stage returns `IAsyncEnumerable<ContentItem>`, the pipeline is fully streaming. Items flow through one at a time. A markdown file can be discovered, parsed, and rendered before the next file is even discovered. There is no intermediate "collect everything into a list" step between stages.

This streaming design keeps memory usage proportional to the size of a single content item, not the size of the entire site. A site with a thousand pages does not need a thousand parsed items in memory simultaneously.

## FailedItem Propagation

This is the most important design decision in the pipeline.

Once an item becomes a `FailedItem`, it stays a `FailedItem`. Every stage checks for failures first and passes them through unchanged:

```
Discover: file.md ---------> DiscoveredItem
Parse:    DiscoveredItem ---> FailedItem (bad YAML)
Render:   FailedItem -------> FailedItem (unchanged, not even attempted)
Generate: FailedItem -------> error in BuildReport
```

Three properties follow from this:

1. **No silent failures.** Every problem surfaces in the build report. A bad YAML header in one file does not cause that file to quietly disappear from the output.

2. **No cascading errors.** A failed Parse does not produce a confusing Render error. The original error message is preserved exactly as it was created. You see "Parse failed: invalid YAML at line 3," not "Render failed: object reference null."

3. **No lost context.** The `ContentRoute` travels with the failure from the moment it occurs through to the build report. You always know which file failed and why.

The pipeline does not throw exceptions for content errors. Exceptions are caught at each stage boundary and converted to `FailedItem` records with a `ContentError` that preserves both the message and the original exception. The only exceptions that escape the pipeline are infrastructure failures -- DI resolution problems, out of memory, disk full -- things that mean the pipeline itself is broken, not just one piece of content.

## BuildReport and BuildDiagnostic

The `BuildReport` class collects the results of a full pipeline run. It tracks five lists:

- **`GeneratedPages`** -- pages that made it through successfully.
- **`SkippedPages`** -- pages filtered out as drafts.
- **`FailedPages`** -- pages that produced errors.
- **`Diagnostics`** -- structured messages with severity levels.
- **`BrokenLinks`** -- internal links pointing to routes that do not exist.

`BuildDiagnostic` is a record, not a union type. It uses a `DiagnosticSeverity` enum to distinguish between `Info`, `Warning`, and `Error`:

```csharp
public enum DiagnosticSeverity { Info, Warning, Error }

public record BuildDiagnostic(
    DiagnosticSeverity Severity,
    ContentRoute? Route,
    string Message,
    Exception? Exception = null,
    string? SourceFile = null);
```

The `Route` is nullable because some diagnostics are not tied to a specific page (e.g., a global configuration problem). The `SourceFile` field provides an alternative location identifier when a route is not available.

`BuildReport.HasErrors` gives a single pass/fail signal:

```csharp
public bool HasErrors => Diagnostics.Any(d => d.Severity is DiagnosticSeverity.Error)
                      || BrokenLinks.Count > 0
                      || FailedPages.Count > 0;
```

When the static site generator runs in CI, it checks `HasErrors` and sets a non-zero exit code on failure. Any content error, any broken internal link, any page that failed to render -- the build fails. `WriteTo(Console.Out)` produces a human-readable summary with page counts, timing, and itemized errors and warnings.

## Extending the Pipeline

### Adding a new content source

Implement `IContentService` and register it with DI. Your `DiscoverAsync` method yields `DiscoveredItem` records with whatever `ContentSource` case fits your content. The pipeline calls your service alongside the built-in ones and feeds the items through the same Parse/Render/Generate stages.

For content that does not come from markdown files, use `ProgrammaticSource` with an `IProgrammaticContentGenerator`:

```csharp
public interface IProgrammaticContentGenerator
{
    Task<ProgrammaticContent> GenerateAsync(ContentRoute route);
}
```

Your generator produces front matter and raw content. The parser handles the rest.

See <xref:penn.guides.custom-content-service> for a full walkthrough.

### Adding a custom parser

Implement `IContentParser`. The interface has a single method:

```csharp
Task<ContentItem> ParseAsync(DiscoveredItem item);
```

Return a `ParsedItem` on success or a `FailedItem` on failure. The pipeline wraps your result in a `ContentItem` and passes it downstream.

The union type design means you cannot introduce a new pipeline state without updating every consumer. Add a fifth case to `ContentItem` and the compiler finds every `switch` expression that needs updating. That guarantee does not come from convention or code review -- it comes from the type system.

For a broader look at how the pipeline fits into the dev server and static build modes, see <xref:penn.under-the-hood.dev-vs-deployment-architecture>.

---
title: "Content Processing Pipeline"
description: "How Penn transforms content from discovery to rendered HTML using C# 15 union types and a streaming pipeline"
uid: "penn.under-the-hood.content-processing-pipeline"
order: 3010
---

Penn's content pipeline is the heartbeat of the engine. It takes raw content -- markdown files, Razor pages, redirects, whatever you throw at it -- and transforms it into rendered HTML through a series of type-safe stages. The secret weapon? C# 15 union types that make illegal states unrepresentable and force you to handle every case. No more `null` checks buried three layers deep. No more "what kind of content item is this, exactly?" The compiler tells you.

## The ContentItem Union

At the center of everything sits `ContentItem`, a union type with exactly four cases:

```csharp:xmldocid
T:Penn.Pipeline.ContentItem
```

Each case is a plain record:

```csharp:xmldocid
T:Penn.Pipeline.DiscoveredItem
T:Penn.Pipeline.ParsedItem
T:Penn.Pipeline.RenderedItem
T:Penn.Pipeline.FailedItem
```

The union enforces exhaustive matching. If you write a `switch` over `ContentItem` and forget a case, the compiler yells at you. This is not a guideline or a code review comment -- it is a compile error. Every pipeline stage must decide what to do with every possible state.

Construction is explicit:

```csharp
var item = new ContentItem(new DiscoveredItem(route, source));
```

And pattern matching is the primary way to work with items:

```csharp
var message = item switch
{
    DiscoveredItem d => $"Found: {d.Route}",
    ParsedItem p     => $"Parsed: {p.Route} with {p.Metadata.Title}",
    RenderedItem r   => $"Rendered: {r.Route}",
    FailedItem f     => $"Failed: {f.Route} - {f.Error.Message}",
};
```

Notice the shared `Route` property on the union itself. Every case carries a `ContentRoute`, and the union exposes it directly so you don't have to pattern match just to find out *where* something lives.

## The Four Pipeline Stages

The pipeline is a linear chain of `IAsyncEnumerable<ContentItem>` streams:

```
Discover --> Parse --> Render --> Generate
```

Each stage receives the output of the previous one, transforms what it can, and passes everything else through. Here is the full flow in ASCII art, because we are nothing if not tasteful:

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

### Stage 1: Discover

Content services (`IContentService` implementations) yield `DiscoveredItem` records. Each one pairs a `ContentRoute` with a `ContentSource`:

```csharp:xmldocid
T:Penn.Pipeline.ContentSource
```

The `ContentSource` union tells the pipeline *how* to find the raw content:

- `MarkdownFileSource` -- a file on disk
- `RazorPageSource` -- a Blazor component type
- `RedirectSource` -- a URL to redirect to
- `ProgrammaticSource` -- content generated in code

The `ContentPipeline` iterates all registered services and yields their discoveries as a flat stream:

```csharp
public async IAsyncEnumerable<ContentItem> DiscoverAsync()
{
    foreach (var service in _services)
    {
        await foreach (var discovered in service.DiscoverAsync())
        {
            yield return new ContentItem(discovered);
        }
    }
}
```

No failures possible here (yet). Discovery is optimistic.

### Stage 2: Parse

The parser receives `DiscoveredItem` records and transforms them into `ParsedItem` records by reading the source, extracting front matter, and pulling out the raw markdown body.

For markdown files, `MarkdownContentParser<TFrontMatter>` reads the file, runs it through `FrontMatterParser`, and produces:

```csharp
new ContentItem(new ParsedItem(item.Route, metadata, result.Body))
```

If anything goes wrong -- file not found, YAML parsing error, cosmic rays -- the item becomes a `FailedItem`:

```csharp
new ContentItem(new FailedItem(item.Route,
    new ContentError($"Failed to parse {path}: {ex.Message}", ex)))
```

The critical pattern: **FailedItems pass through unchanged**. The pipeline never tries to "fix" or retry a failed item. It just carries it forward so the Generate stage can report what went wrong.

```csharp
if (item is FailedItem)
{
    yield return item;
    continue;
}
```

This is not laziness -- it is a design choice. A failed item at the Parse stage should not silently disappear. It should show up in the build report so you know something is broken.

### Stage 3: Render

The renderer takes `ParsedItem` records and transforms them into `RenderedItem` records. For markdown content, `MarkdownContentRenderer` runs the raw markdown through the Markdig pipeline, which handles:

- Syntax highlighting (via `CodeHighlightRenderer` and `HighlightingService`)
- Tabbed code blocks
- Custom alert blocks
- Heading ID generation
- All the usual Markdig advanced extensions

The output is a `RenderedContent` record containing the HTML, an outline (for table-of-contents navigation), tags, cross-references, search data, and social metadata.

Same failure pattern as Parse -- exceptions produce `FailedItem` records that propagate forward.

### Stage 4: Generate

The final stage consumes the stream and produces a `BuildReport`. This is where the pipeline makes its final decisions:

```csharp
switch (item)
{
    case RenderedItem rendered:
        if (rendered.Metadata is IDraftable { IsDraft: true })
        {
            reportBuilder.AddSkippedPage(rendered.Route);
        }
        else
        {
            reportBuilder.AddGeneratedPage(rendered.Route);
        }
        break;

    case FailedItem failed:
        reportBuilder.AddError(failed.Route, failed.Error.Message, failed.Error.Exception);
        break;

    default:
        reportBuilder.AddWarning(item.Route, "Item did not complete pipeline");
        break;
}
```

Two things worth noting:

1. **Draft filtering happens here**, not earlier. Draft items go through the entire pipeline -- they get parsed, rendered, the works. They only get filtered at the output stage. This means you can preview drafts during development (where Generate is not the final arbiter) while ensuring they never appear in production builds.

2. **Items that didn't reach RenderedItem state** get warnings. If something is still a `DiscoveredItem` or `ParsedItem` by the time it reaches Generate, something went wrong in the pipeline -- maybe a stage skipped it.

## The Convenience Method

For the common case of "just run everything," `ContentPipeline.RunAsync` chains the stages together:

```csharp
public async Task<BuildReport> RunAsync(OutputOptions options)
{
    var discovered = DiscoverAsync();
    var parsed = ParseAsync(discovered);
    var rendered = RenderAsync(parsed);
    return await GenerateAsync(rendered, options);
}
```

The beauty of `IAsyncEnumerable` is that this is fully streaming. Items flow through the pipeline one at a time -- there is no "collect all items, then parse them all, then render them all." A markdown file can be discovered, parsed, and rendered before the next file is even discovered.

## FailedItem Propagation

This deserves its own section because it is the single most important design decision in the pipeline.

Once an item becomes a `FailedItem`, it stays a `FailedItem`. Every stage checks for failures first and passes them through:

```
Discover: file.md --> DiscoveredItem
Parse: DiscoveredItem --> FailedItem (bad YAML)
Render: FailedItem --> FailedItem (unchanged, not even attempted)
Generate: FailedItem --> error in BuildReport
```

This means:

- **No silent failures.** Every problem surfaces in the build report.
- **No cascading errors.** A failed Parse does not cause a confusing Render error. The original error message is preserved.
- **No lost context.** The `ContentRoute` travels with the failure, so you always know *which* file failed and *why*.

The pipeline does not throw exceptions for content errors. Exceptions are caught at each stage boundary and converted to `FailedItem` records. The only exceptions that escape the pipeline are infrastructure failures (DI resolution problems, out of memory, etc.) -- things that mean the pipeline itself is broken, not just one piece of content.

## BuildDiagnostic: The Other Union

The build report uses its own union type for diagnostics:

```csharp
public union BuildDiagnostic(DiagnosticInfo, DiagnosticWarning, DiagnosticError)
```

Just like `ContentItem`, the compiler enforces exhaustive handling. Info messages, warnings, and errors are all distinct types -- you cannot accidentally treat a warning as an error or vice versa. The `BuildReport.HasErrors` property checks for `DiagnosticError` instances and broken links, giving you a clean pass/fail signal for CI pipelines.

## Extending the Pipeline

Want to add a new content source? Implement `IContentService` and yield `DiscoveredItem` records from `DiscoverAsync()`. The pipeline does not care where content comes from -- it only cares about the `ContentItem` union.

Want a custom parser? Implement `IContentParser`. The pipeline will call your parser for items that match the source type you handle.

The union type design means you cannot accidentally introduce a new pipeline state without updating every consumer. Add a fifth case to `ContentItem` and the compiler will find every `switch` expression that needs updating. That is not something an abstract base class can promise you.

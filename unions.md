# Using C# 15 union types in a content engine

I've been working on [Pennington](https://github.com/example/pennington), a content engine that turns markdown, Razor pages, and programmatic content into static documentation sites. When C# 15 shipped union types, I wanted to try them out on a real problem. Here's what I found.

This post walks through four patterns where unions helped, one case where they didn't, and the practical details I wish I'd known upfront.

## What we're building

Pennington has a content pipeline with four stages:

1. **Discover** content (find markdown files, Razor pages, etc.)
2. **Parse** front matter and body text
3. **Render** to HTML with outlines and cross-references
4. **Generate** the final static output

At each stage, a piece of content is in a different state, carrying different data. A discovered item has a file path. A parsed item has metadata and raw markdown. A rendered item has HTML. And at any point, something can go wrong.

The question is: how do you represent those states in code?

## Before unions: the options weren't great

Before unions, we had two choices:

- **A class hierarchy** with a base `ContentItem` class and subtypes for each stage. This works, but you need runtime casts to access stage-specific data, and nothing stops you from forgetting a case in a switch statement.
- **A single class with nullable fields.** Set `Html` to null until the rendering stage, set `Error` to null unless something fails. Hope that nobody reads `Html` before it's populated.

Both approaches push errors to runtime. Union types move them to compile time.

## Declare the union

Here's what the `ContentItem` union looks like:

```csharp
public record DiscoveredItem(ContentRoute Route, ContentSource Source);
public record ParsedItem(ContentRoute Route, IFrontMatter Metadata, string RawMarkdown);
public record RenderedItem(ContentRoute Route, IFrontMatter Metadata, RenderedContent Content);
public record FailedItem(ContentRoute Route, ContentError Error);

public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem);
```

Each case type is an ordinary record. The `union` keyword declares that a `ContentItem` is exactly one of these four types, and no others can be added from outside.

When you write a switch expression over a `ContentItem`, the compiler checks that you've handled every case. Add a fifth case type next month, and the compiler flags every switch that doesn't cover it.

## Pattern 1: Content flowing through the pipeline

Each pipeline stage receives a stream of `ContentItem` values and produces a new stream:

```csharp
public async IAsyncEnumerable<ContentItem> ParseAsync(
    IAsyncEnumerable<ContentItem> items)
{
    await foreach (var item in items)
    {
        if (item is FailedItem)
        {
            yield return item;  // pass through unchanged
            continue;
        }

        if (item is DiscoveredItem discovered)
        {
            try
            {
                result = await _parser.ParseAsync(discovered);
            }
            catch (Exception ex)
            {
                result = new FailedItem(discovered.Route,
                    new ContentError($"Parse failed: {ex.Message}", ex));
            }
            yield return result;
        }
        else
        {
            yield return item;  // already parsed or rendered
        }
    }
}
```

A few things to note:

- **`FailedItem` is a dead-end state.** Once content fails, it passes through every subsequent stage untouched. It isn't dropped -- it flows to the final report, carrying its error message. The type system guarantees a `FailedItem` won't accidentally be treated as a `ParsedItem`.
- **Implicit conversion.** We write `new FailedItem(...)` and assign it directly to a `ContentItem` variable. The compiler generates an implicit conversion from each case type to the union. No wrapping needed.

## Pattern 2: Multiple content sources

Content in Pennington can come from four different sources:

```csharp
public record MarkdownFileSource(FilePath Path);
public record RazorPageSource(string ComponentType);
public record RedirectSource(UrlPath TargetUrl);
public record ProgrammaticSource(IProgrammaticContentGenerator Generator);

public union ContentSource(
    MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource);
```

The markdown parser uses a pattern match to check whether it received a source it can handle:

```csharp
if (item.Source is not MarkdownFileSource markdownSource)
{
    return new FailedItem(item.Route,
        new ContentError("Unsupported content source type for parser"));
}

var filePath = markdownSource.Path.Value;
var content = await _fileSystem.File.ReadAllTextAsync(filePath);
```

This is a guard clause, not a cast. If the source isn't markdown, we get a clean `FailedItem`, not a `ClassCastException`.

## Pattern 3: Expose shared properties on the union body

Every `ContentItem` case carries a `Route`. Rather than pattern-matching every time you need it, expose the property directly on the union:

```csharp
public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem)
{
    public ContentRoute Route => this switch
    {
        DiscoveredItem d => d.Route,
        ParsedItem p     => p.Route,
        RenderedItem r   => r.Route,
        FailedItem f     => f.Route,
        null => throw new InvalidOperationException("Uninitialized ContentItem")
    };
}
```

**Why `null` instead of `_`?** Unions are structs, and the default value of a struct has a null `Value`. The `null` arm handles that explicitly. If we used `_` (a discard), it would also swallow any future case type we add -- the compiler wouldn't warn us. Using `null` preserves exhaustiveness checking for the real cases.

## Pattern 4: Link verification results

When Pennington checks links in rendered HTML, there are three possible outcomes:

```csharp
public record ValidLink(ContentRoute SourcePage, string Url);
public record BrokenLinkResult(ContentRoute SourcePage, string Url,
    LinkType Type, string Reason);
public record ExternalLink(ContentRoute SourcePage, string Url);

public union LinkCheckResult(ValidLink, BrokenLinkResult, ExternalLink);
```

The classification logic is straightforward:

```csharp
if (IsExternalUrl(url))
    return new ExternalLink(sourcePage, url);

if (_knownPaths.Contains(normalizedUrl))
    return new ValidLink(sourcePage, url);

return new BrokenLinkResult(sourcePage, url, linkType, "Page not found");
```

Each return uses implicit conversion. No factory methods, no wrapper constructors. The type *is* the classification.

## When not to use a union

We initially made `BuildDiagnostic` a union with three case types: `DiagnosticInfo`, `DiagnosticWarning`, and `DiagnosticError`. All three carried the same fields -- `Route`, `Message`, `SourceFile`. The only difference was that errors also had an `Exception`.

The union body had to expose each shared property through a switch expression that did the same thing for every case. That's a sign the union isn't earning its keep.

We replaced it with a record and an enum:

```csharp
public enum DiagnosticSeverity { Info, Warning, Error }

public record BuildDiagnostic(
    DiagnosticSeverity Severity,
    ContentRoute? Route,
    string Message,
    Exception? Exception = null,
    string? SourceFile = null);
```

**The rule of thumb:** use a union when the cases have different shapes -- different fields, different operations, where confusing one for another is a real mistake. Use a record with an enum when the cases carry the same data and differ only in classification.

## Quick reference: what I wish I'd known

Here's a summary of the practical details that tripped me up or saved me time.

| Topic | What to do |
|---|---|
| **Construction** | Use implicit conversion: `ContentItem item = new FailedItem(...)`. No need to wrap in `new ContentItem(...)`. |
| **Switch completeness** | Handle `null` (for uninitialized structs), not `_`. The discard hides missing cases. |
| **Union body members** | Expose shared properties on the union to avoid repetitive pattern matching at call sites. |
| **`Task.FromResult`** | Use `Task.FromResult<ContentItem>(new FailedItem(...))` -- the generic parameter gives the compiler a target type for the implicit conversion. |
| **`is` pattern matching** | Works on unions: `if (item is FailedItem failed)` unwraps and extracts in one step. |
| **When to use an enum instead** | If all cases carry the same fields, a record with an enum is simpler. |

## Wrapping up

The biggest win from union types isn't any single pattern. It's the exhaustiveness guarantee. When you add a new case type, the compiler immediately tells you every switch expression that needs updating. That's the kind of safety net that matters most in a codebase that grows over time.

If you're working with a pipeline, a classifier, or anything where a value can be "one of these N things and nothing else," give unions a try. Start with one, keep it small, and see if the compiler warnings save you a bug or two.

Have you tried union types in your own projects? I'd love to hear what patterns you've found useful.

---
title: "ContentSource: constructing and pattern-matching the union"
description: "How to construct a ContentSource case in DiscoverAsync, and how to consume one with C# 15 pattern matching — including the net10.0 compatibility shim."
uid: explanation.core.content-source
order: 301015
sectionLabel: "Core Architecture"
tags: [pipeline, unions, content-source]
---

`ContentSource` is the second of Pennington's two pipeline unions: where `ContentItem` discriminates a page's stage, `ContentSource` discriminates *where the page came from*. The five cases — `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `ProgrammaticSource`, `EndpointSource` — capture every origin Pennington ships. <xref:explanation.core.content-pipeline> covers why the pipeline is shaped as a union at all; this page focuses on the two questions that come up the moment you write a custom `IContentService`: *how do I build one of these?* and *how do I read one back out?*

## Constructing a `ContentSource`

The union has implicit conversions from each case type, so the shorthand form lets the case stand alone:

```csharp
yield return new DiscoveredItem(route, new MarkdownFileSource(filePath));
yield return new DiscoveredItem(route, new RazorPageSource(typeof(MyComponent).AssemblyQualifiedName!));
yield return new DiscoveredItem(route, new RedirectSource(new UrlPath("/new-home/")));
yield return new DiscoveredItem(route, new ProgrammaticSource(generator));
yield return new DiscoveredItem(route, new EndpointSource());
```

When the union wrap matters for clarity — for example, when the case is computed in a conditional — the explicit constructor form is also available:

```csharp
yield return new DiscoveredItem(route, new ContentSource(new EndpointSource()));
```

Both forms produce the same value. Pick whichever reads better at the call site; the implicit conversion is not a special case the runtime treats differently.

### Which case to use

| Case | Use when |
|---|---|
| `MarkdownFileSource(FilePath)` | The page is a markdown file the parser/renderer should walk. |
| `RazorPageSource(string componentType)` | The page is a Razor component matched at request time by Blazor routing. |
| `RedirectSource(UrlPath targetUrl)` | The route is an explicit 30x redirect to another URL. |
| `ProgrammaticSource(IProgrammaticContentGenerator)` | The page body is computed in code by a generator (no markdown, no Razor). |
| `EndpointSource()` | A sibling `MapGet` produces the HTML; the build crawler needs to discover the URL but Pennington's parser/renderer is not involved. |

`RedirectSource` and `EndpointSource` both *exclude* the route from `sitemap.xml`. The reason differs — `RedirectSource` has no canonical body of its own, and `EndpointSource`'s canonical body is owned by the endpoint, not by the content service — but the visible effect is the same: don't reach for either when you want the route to appear in the sitemap.

## Reading a `ContentSource` back out

The union exposes a `Value` property holding the wrapped case instance — the same shape on both `net11.0` (where the C# 15 `union` keyword synthesizes it) and `net10.0` (where a hand-written polyfill struct provides it). Pattern matching reaches through `Value` to the case type:

```csharp
public string Describe(ContentSource source) => source.Value switch
{
    MarkdownFileSource markdown => $"markdown file at {markdown.Path.Value}",
    RazorPageSource razor => $"Razor component {razor.ComponentType}",
    RedirectSource redirect => $"redirect to {redirect.TargetUrl.Value}",
    ProgrammaticSource programmatic => $"generator {programmatic.Generator.GetType().Name}",
    EndpointSource => "endpoint-rendered route",
    _ => "unknown",
};
```

For one specific case, the `is` form is just as clean:

```csharp
if (source.Value is RedirectSource redirect)
{
    Response.Redirect(redirect.TargetUrl.Value);
    return;
}
```

`SitemapService` filters out the two non-canonical cases in a single expression — the canonical idiom for "skip routes that aren't real HTML":

```csharp
if (discovered.Source.Value is RedirectSource or EndpointSource) continue;
```

The same shape works for the other Pennington unions: `item.Value is ParsedItem parsed`, `renderResult.Value is FailedItem failed`, and so on. Treat `.Value` as the entry point and the case types as what you actually pattern-match against.

### Why `.Value` and not the case type directly

Pennington multi-targets `net10.0;net11.0`. On `net10.0` the polyfill is a `readonly struct` that holds the case in an `object? Value` field; on `net11.0+` the C# 15 `union` keyword synthesizes the equivalent. Going through `.Value` is the one shape that compiles unchanged on both TFMs and matches what every consumer in the codebase already does. Avoid the polyfill-only constructors-as-pattern-source shortcut — it only works on `net10.0` and breaks the moment a reader looks at the `net11.0` build.

## See also

- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
- How-to: [Source content from outside the file system](xref:how-to.extensibility.custom-content-service)
- Reference: [Sitemap configuration](xref:how-to.configuration.sitemap)

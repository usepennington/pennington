---
title: "Source content from outside the file system"
description: "Implement IContentService to surface JSON files, a database table, or a remote API as routed pages, navigation entries, search documents, and xref targets."
uid: how-to.extensibility.custom-content-service
order: 203010
sectionLabel: Extensibility
tags: [extensibility, content-service, pipeline, cross-references]
---

To source content from somewhere `MarkdownContentService<T>` can't reach — a folder of JSON release notes, a SQL table, a remote API, generated API reference — and have those pages appear in navigation, cross-references, search, and the static build the same way markdown pages do, implement `IContentService` directly. The recipe below uses the `ReleaseNotesContentService` from `examples/ExtensibilityLabExample`, which turns `Content/releases/*.json` into `/releases/{version}/` routes. For a second markdown tree with a different front-matter type, use chained `AddMarkdownContent<T>` instead — see <xref:how-to.configuration.multiple-sources>.

## Before you begin

- A working Pennington site on bare `AddPennington` (see <xref:tutorials.getting-started.first-site> if not) — `AddDocSite` pins its own markdown service, so adding a second content service on top works, but the concepts below assume familiarity with the unwrapped host.
- Familiarity with the four-stage pipeline at a conceptual level (<xref:explanation.core.content-pipeline>).
- Source data that can be enumerated synchronously or asynchronously on startup — `DiscoverAsync` runs both at build time and on demand for live requests.

For a working setup, see `examples/ExtensibilityLabExample` — the `ReleaseNotesContentService` file is self-contained.

## Model the source records

Define an immutable record that represents one page's worth of source data. `ReleaseEntry` is the JSON-backed shape the rest of the service keys off; the equivalent type in another project carries whatever fields the source provides.

```csharp:xmldocid
T:ExtensibilityLabExample.ReleaseEntry
```

## Implement the service

Create a sealed class implementing <xref:reference.api.i-content-service>, inject whatever reads the source (here, `IWebHostEnvironment` for `ContentRootPath`), and cache the parsed records in a `Lazy<ImmutableList<T>>` so discovery and the TOC share one pass over the source. The full example lives in `examples/ExtensibilityLabExample/ReleaseNotesContentService.cs`; the snippets below cover each member.

`DiscoverAsync` yields one `DiscoveredItem` per page. Build each item's `ContentRoute` through `ContentRouteFactory.FromUrl` (synthetic URL, no backing file) or `ContentRouteFactory.FromCustom` (URL plus an on-disk `FilePath` so file-watching picks up edits), and pair it with a `ContentSource` case. Use `EndpointSource` when an endpoint elsewhere in `Program.cs` produces the HTML — the build crawler still discovers the URL and fetches it through the live pipeline, but the route is excluded from `sitemap.xml` because the canonical HTML is owned by the endpoint, not by this service. Reach for `RedirectSource(targetUrl)` only for genuine 30x redirects to another URL.

`ContentSource` is a union that wraps exactly one of `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `ProgrammaticSource`, or `EndpointSource` — it is not a base class. Implicit conversions from each case type make the shorthand form work; pick whichever reads more clearly.

```csharp
// Explicit — the union wrap is visible
yield return new DiscoveredItem(route, new ContentSource(new EndpointSource()));

// Shorthand — implicit conversion wraps the case for you
yield return new DiscoveredItem(route, new EndpointSource());
```

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.DiscoverAsync
```

`GetContentTocEntriesAsync` returns one `ContentTocItem` per row in the sidebar and document in the search index. Set `Title`, `Route`, `Order` (use tidy 10/20/30 sequences), `HierarchyParts` (the path segments that drive sidebar nesting), and `SectionLabel` (group header). Return an index entry first, then one per record.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentTocEntriesAsync
```

`GetContentToCopyAsync` is for static assets copied verbatim into the output tree (images, downloads). `GetContentToCreateAsync` is for dynamically generated files that are not routes the crawler will visit — the `LlmsTxtContentService` uses it for stripped-markdown sidecars. For a service whose only output is HTML served by a `MapGet` endpoint, both return `ImmutableList.Empty`.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentToCopyAsync
```

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentToCreateAsync
```

`GetCrossReferencesAsync` publishes one `CrossReference(uid, title, route)` per record, registering a uid that other content can target with `<xref:uid>` or `href="xref:uid"`. Pick a stable prefix (`release-1.0.0` here) so authors can deep-link specific entries without pasting URLs that may change.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetCrossReferencesAsync
```

## Register the implementation

`AddPennington` does not auto-discover `IContentService` implementations — register directly on `IServiceCollection`. When an endpoint in `Program.cs` needs the concrete type to render detail pages, register it once by concrete type and forward `IContentService` to the same instance so the container does not create a second copy.

```csharp
builder.Services.AddSingleton<ReleaseNotesContentService>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<ReleaseNotesContentService>());
```

## Result

The discovered records produce a "Releases" section in the sidebar, one route per entry, and one xref id per entry:

```text
/releases/                  -> Releases (index)
/releases/1.0.0/            -> uid: release-1.0.0
/releases/1.1.0/            -> uid: release-1.1.0
```

Each `/releases/{version}/` URL renders through the sibling `MapGet` endpoint, the entries appear in the search index under the "Releases" section, and `<xref:release-1.0.0>` in any markdown page resolves to `/releases/1.0.0/`.

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/releases/` — the index lists every entry and each `/releases/{version}/` renders.
- The "Releases" section shows up in navigation with one child per discovered record.
- Authoring `<xref:release-1.0.0>` inside a markdown page resolves to the right URL in the rendered output, and the static build (`dotnet run -- build`) writes one HTML file per route under `output/releases/`.

## Related

- Reference: [Content pipeline interfaces](xref:reference.api.i-content-service)
- Reference: [Routing types](xref:reference.api.content-route)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)

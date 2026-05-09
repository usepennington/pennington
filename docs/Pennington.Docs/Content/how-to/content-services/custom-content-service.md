---
title: "Source content from outside the file system"
description: "Implement IContentService to surface JSON files, a database table, or a remote API as routed pages, navigation entries, search documents, and xref targets."
uid: how-to.content-services.custom-content-service
order: 208010
sectionLabel: "Content Services"
tags: [extensibility, content-service, pipeline, cross-references]
---

To source content from somewhere `MarkdownContentService<T>` can't reach — a folder of JSON release notes, a SQL table, a remote API, generated API reference — and have those pages appear in navigation, cross-references, search, and the static build the same way markdown pages do, implement `IContentService` directly. The recipe below uses the `ReleaseNotesContentService` from `examples/ExtensibilityLabExample`, which turns `Content/releases/*.json` into `/releases/{version}/` routes. For a second markdown tree with a different front-matter type, use chained `AddMarkdownContent<T>` instead — see <xref:how-to.discovery.multiple-sources>.

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

Create a sealed class implementing <xref:reference.api.i-content-service>, inject whatever reads the source (here, `IWebHostEnvironment` for `ContentRootPath`), and cache the parsed records in a `Lazy<ImmutableList<T>>` so discovery and the TOC share one pass over the source.

Five members carry everything this how-to needs:

- `DiscoverAsync` yields one `DiscoveredItem` per page. Build each item's `ContentRoute` with `ContentRouteFactory.FromUrl` (synthetic URL, no backing file) or `ContentRouteFactory.FromCustom` (URL plus an on-disk `FilePath` so file-watching picks up edits), then pair the route with a `ContentSource` case. `EndpointSource` is used here so the build crawler fetches each URL through a sibling `MapGet` endpoint; the route is excluded from `sitemap.xml` because the canonical HTML is owned by the endpoint.
- `GetContentTocEntriesAsync` returns one `ContentTocItem` per row for the sidebar and the search index. Set `Title`, `Route`, `Order` (tidy 10/20/30 sequences), `HierarchyParts` (sidebar nesting), and `SectionLabel` (group header).
- `GetContentToCopyAsync` and `GetContentToCreateAsync` cover static assets (copied verbatim) and dynamically-generated sidecar files; both return `ImmutableList.Empty` when HTML served by an endpoint is the only output. `LlmsTxtContentService` uses the latter for stripped-markdown sidecars.
- `GetCrossReferencesAsync` publishes one `CrossReference(uid, title, route)` per record so authors can deep-link specific entries with `<xref:uid>`. Pick a stable prefix (`release-1.0.0` here) so the uid does not depend on a URL that may move.

`ContentSource` is a union over `MarkdownFileSource`, `RazorPageSource`, `RedirectSource`, `ProgrammaticSource`, and `EndpointSource` — implicit conversions make the case-name shorthand work, so `new EndpointSource()` and `new ContentSource(new EndpointSource())` are equivalent.

```csharp:xmldocid
T:ExtensibilityLabExample.ReleaseNotesContentService
```

For full member signatures (return types, default implementations, and the parent `IContentEmitter` interface), see <xref:reference.api.i-content-service>.

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
- Background: [When is DocSite the right starting point?](xref:explanation.positioning.docsite-positioning)

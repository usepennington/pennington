---
title: "Source content from outside the file system"
description: "Implement IContentService to surface JSON files, a database table, or a remote API as routed pages, navigation entries, search documents, and xref targets."
uid: how-to.content-services.custom-content-service
order: 1
sectionLabel: "Content Services"
tags: [extensibility, content-service, pipeline, cross-references]
---

To source content from somewhere `MarkdownContentService<T>` can't reach — a folder of JSON release notes, a SQL table, a remote API, generated API reference — and have those pages appear in navigation, cross-references, search, and the static build the same way markdown pages do, implement `IContentService` directly. For a second markdown tree with a different front-matter type, use chained `AddMarkdownContent<T>` instead — see <xref:how-to.discovery.multiple-sources>. When a dataset feeds existing pages but needs no routes of its own, register it with `AddDataFile<T>` rather than a content service — see <xref:how-to.content-services.data-files>.

The recipe references `examples/ExtensibilityLabExample/ReleaseNotesContentService.cs`, which turns `Content/releases/*.json` into `/releases/{version}/` routes.

## Before you begin

- A working Pennington site on bare `AddPennington` (see <xref:tutorials.getting-started.first-site> if not). `AddDocSite` pins its own markdown service, so adding a second content service on top works, but the concepts below assume the unwrapped host.
- Familiarity with the four-stage pipeline at a conceptual level (<xref:explanation.core.content-pipeline>).
- Source data that can be enumerated synchronously or asynchronously on startup — `DiscoverAsync` runs both at build time and on demand for live requests.

## Write the service

Implement <xref:reference.api.i-content-service> as a sealed class. Cache the parsed records in a `Lazy<ImmutableList<T>>` so discovery and the TOC share one pass over the source.

```csharp:symbol
examples/ExtensibilityLabExample/ReleaseNotesContentService.cs
```

Three non-obvious moves carry this service:

- **`DiscoverAsync` pairs a route with a `ContentSource` case.** Build the route with `ContentRouteFactory.FromUrl` (synthetic URL, no backing file) or `ContentRouteFactory.FromCustom` (URL plus an on-disk `FilePath` so file-watching picks up edits). The example uses `EndpointSource` so the build crawler fetches each URL through a sibling `MapGet` endpoint; those routes serve real canonical HTML, so they appear in `sitemap.xml` like any other page.
- **`GetContentTocEntriesAsync` feeds the sidebar and the search index.** Set `Title`, `Route`, `Order` (10/20/30 spacing), `HierarchyParts` (sidebar nesting), and `SectionLabel` (group header). The same items power search ranking.
- **`GetCrossReferencesAsync` publishes one `CrossReference(uid, title, route)` per record** so authors can deep-link entries with `<xref:uid>`. Pick a stable prefix (`release-1.0.0` here) so the uid does not depend on a URL that may move.

`GetContentToCopyAsync` and `GetContentToCreateAsync` return `ImmutableList.Empty` when HTML served by an endpoint is the only output.

## Register the service

`AddPennington` does not auto-discover `IContentService` implementations — register directly on `IServiceCollection`. When an endpoint in `Program.cs` needs the concrete type to render detail pages, register it once by concrete type and forward `IContentService` to the same instance so the container does not create a second copy.

```csharp
builder.Services.AddSingleton<ReleaseNotesContentService>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<ReleaseNotesContentService>());
```

This service reads its JSON into a `Lazy<T>` once and lives as a singleton, so an edit to a release file during a dev session is not picked up until restart. When live-reload on source edits matters, register the service file-watched instead — `AddFileWatched<T>` plus a *transient* `IContentService` wrapper, as <xref:how-to.feeds.custom-feed> shows. `AddSingleton<IContentService>` over a file-watched type silently caches the first copy and serves stale data. To source from a remote API rather than disk, see <xref:how-to.content-services.remote-api>.

## Feed your records to taxonomy, search, and JSON-LD

The service above produces routes, navigation, and cross-references. To also let your records drive browse-by-field pages, custom search facets, and JSON-LD — the same way markdown records do — give each record a typed front matter that implements the capability mixins, and **attach it to the discovered item**. The example's `ReleaseEntry` does exactly this:

```csharp:symbol
examples/ExtensibilityLabExample/ReleaseNotesContentService.cs > ReleaseEntry
```

```csharp
yield return new DiscoveredItem(route, new EndpointSource()) { Metadata = entry };
```

That single `Metadata` assignment is the seam: the engine reads it through `GetRecordsAsync` (the default bridges from `DiscoverAsync`, so attaching metadata is all it takes — no override needed) and every discovery pillar lights up:

- **Taxonomy** — `AddTaxonomy<ReleaseEntry, string>(opts => opts.SelectKey = fm => fm.Channel)` gives you `/channel/` browse pages with no `MarkdownFileSource` required (see <xref:how-to.content-services.taxonomy>).
- **Search facets** — the `IHasSearchFacets` `channel` axis emits alongside the built-in `section`/`tag`/`area` dimensions.
- **JSON-LD** — the `IHasStructuredData` entity is injected into each release page's `<head>` automatically when `CanonicalBaseUrl` is set; no `<script>` to hand-write.

A record participates in a taxonomy axis only when its metadata *is* that axis's `TFrontMatter`, so type your records as the front matter you intend to browse. If you project records that don't flow through `DiscoverAsync` (or want to filter which ones do), override `GetRecordsAsync` directly instead of attaching `Metadata`.

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
- Authoring `<xref:release-1.0.0>` inside a markdown page resolves to the right URL, and the static build (`dotnet run -- build`) writes one HTML file per route under `output/releases/`.

## Related

- Reference: [Content pipeline interfaces](xref:reference.api.i-content-service)
- Reference: [Routing types](xref:reference.api.content-route)
- How-to: [Source content from a remote API](xref:how-to.content-services.remote-api)
- How-to: [Use a YAML or JSON data file in pages](xref:how-to.content-services.data-files)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
- Background: [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning)

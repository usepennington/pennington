---
title: "Implement a custom IContentService"
description: "Adapt a non-markdown source — JSON, a database, a remote API — into the pipeline by implementing IContentService and yielding DiscoveredItems, TOC entries, and cross-references."
uid: how-to.extensibility.custom-content-service
order: 203010
sectionLabel: Extensibility
tags: [extensibility, content-service, pipeline, cross-references]
---

When content lives somewhere `MarkdownContentService<T>` can't reach — a folder of JSON release notes, a SQL table, a remote API, generated API reference — and those pages need to appear in navigation, cross-references, search, and the static build the same way markdown pages do, implement `IContentService` directly. The recipe below uses the `ReleaseNotesContentService` from `examples/ExtensibilityLabExample`, which turns `Content/releases/*.json` into `/releases/{version}/` routes. For a second markdown tree with a different front-matter type, use chained `AddMarkdownContent<T>` instead — see <xref:how-to.configuration.multiple-sources>.

## Assumptions

- A working Pennington site on bare `AddPennington` (see <xref:tutorials.getting-started.first-site> if not) — `AddDocSite` pins its own markdown service, so adding a second content service on top works, but the concepts below assume familiarity with the unwrapped host
- Familiarity with the four-stage pipeline at a conceptual level (<xref:explanation.core.content-pipeline>)
- Source data that can be enumerated synchronously or asynchronously on startup — `DiscoverAsync` runs both at build time and on demand for live requests

For a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — the `ReleaseNotesContentService` file is self-contained. This page is a recipe, not a tour of the whole example.

---

## Steps

<Steps>
<Step StepNumber="1">

**Model your source records**

Define an immutable record that represents one page's worth of source data. `ReleaseEntry` is the JSON-backed shape the rest of the service keys off; the equivalent type in another project carries whatever fields the source provides.

```csharp:xmldocid
T:ExtensibilityLabExample.ReleaseEntry
```

</Step>
<Step StepNumber="2">

**Implement `IContentService` and load the records once**

Create a sealed class implementing <xref:reference.extension-points.content-pipeline>, inject whatever reads the source (here, `IWebHostEnvironment` for `ContentRootPath`), and cache the parsed records in a `Lazy<ImmutableList<T>>` so discovery and the TOC share one pass over the source. The full example lives in `examples/ExtensibilityLabExample/ReleaseNotesContentService.cs`; the subsequent steps fence each member in turn.

</Step>
<Step StepNumber="3">

**Yield one `DiscoveredItem` per page from `DiscoverAsync`**

Build each item's `ContentRoute` through `ContentRouteFactory.FromUrl` (synthetic URL, no backing file) or `ContentRouteFactory.FromCustom` (URL plus an on-disk `FilePath` so file-watching picks up edits), and pair it with a `ContentSource` case. Use `RedirectSource` as the placeholder when an endpoint elsewhere in `Program.cs` produces the actual HTML. Yield an index route, then one route per record.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.DiscoverAsync
```

</Step>
<Step StepNumber="4">

**Emit `ContentTocItem`s for navigation and search**

Each `ContentTocItem` is one row in the sidebar and one document in the search index. Set `Title`, `Route`, `Order` (use tidy 10/20/30 sequences), `HierarchyParts` (the path segments that drive sidebar nesting), and `SectionLabel` (group header). Return an index entry first, then one per record.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentTocEntriesAsync
```

</Step>
<Step StepNumber="5">

**Return `ContentToCopy` / `ContentToCreate` lists**

`GetContentToCopyAsync` is for static assets copied verbatim into the output tree (images, downloads). `GetContentToCreateAsync` is for dynamically generated files that are not routes the crawler will visit — the `LlmsTxtContentService` uses it for stripped-markdown sidecars. For a service whose only output is HTML served by a `MapGet` endpoint, both return `ImmutableList.Empty`.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentToCopyAsync
```

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentToCreateAsync
```

</Step>
<Step StepNumber="6">

**Publish xref ids through `GetCrossReferencesAsync`**

Each `CrossReference(uid, title, route)` registers a uid that other content can target with `<xref:uid>` or `href="xref:uid"`. Pick a stable prefix (`release-1.0.0` here) so authors can deep-link specific entries without pasting URLs that may change.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetCrossReferencesAsync
```

</Step>
<Step StepNumber="7">

**Register the service in DI**

`AddPennington` does not auto-discover `IContentService` implementations — register directly on `IServiceCollection`. When an endpoint in `Program.cs` needs the concrete type to render detail pages, register it once by concrete type and forward `IContentService` to the same instance so the container does not create a second copy.

```csharp
builder.Services.AddSingleton<ReleaseNotesContentService>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<ReleaseNotesContentService>());
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/releases/` — the index lists every entry and each `/releases/{version}/` renders
- The "Releases" section shows up in navigation with one child per discovered record
- Authoring `<xref:release-1.0.0>` inside a markdown page resolves to the right URL in the rendered output, and the static build (`dotnet run -- build`) writes one HTML file per route under `output/releases/`

## Related

- Reference: [Content pipeline interfaces](xref:reference.extension-points.content-pipeline)
- Reference: [Routing types](xref:reference.extension-points.routing)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
- Background: [When is DocSite the right starting point?](xref:explanation.core.docsite-positioning)

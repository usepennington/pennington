---
title: "Emit generated output artifacts"
description: "Implement an IContentService whose only job is to write a byte artifact to the output — robots.txt, search-index sidecars, social images — with no pages, TOC entries, or xrefs."
uid: how-to.extensibility.emit-generated-artifacts
order: 203100
sectionLabel: Extensibility
tags: [extensibility, content-service, artifacts]
---

To emit a byte artifact into the output — `robots.txt`, a sitemap variant, a social-image `.png`, a sidecar `.json` search index — that is not a routed page, not in navigation, and not an xref target, implement `IContentService` with `GetContentToCreateAsync` as the only meaningful member. Every other interface member returns empty. For the opposite shape — a service that contributes routed pages, TOC entries, and xrefs from a non-markdown source — see <xref:how-to.extensibility.custom-content-service>.

## Before you begin

- A working Pennington site on bare `AddPennington` (see <xref:tutorials.getting-started.first-site> if not).
- Familiarity with the four-stage pipeline at a conceptual level (<xref:explanation.core.content-pipeline>).

For a working setup, see `examples/ExtensibilityLabExample` — `RobotsTxtContentService.cs` is self-contained and `Program.cs` registers it against a bare `AddPennington` host. `LlmsTxtContentService` in the core library is the production example of the same pattern.

## Implement the service

Create a sealed class implementing <xref:reference.api.i-content-service>. The full example is one type; the sections below walk through the one member that does work and the empty-return shape of the rest.

```csharp:xmldocid
T:ExtensibilityLabExample.RobotsTxtContentService
```

`GetContentToCreateAsync` returns one `ContentToCreate(OutputPath, ContentGenerator, ContentType)` per artifact:

- `OutputPath` is a `FilePath` relative to the output root. `new FilePath("robots.txt")` writes to `/robots.txt`; `new FilePath("assets/og/home.png")` writes to `/assets/og/home.png`.
- `ContentGenerator` is a `Func<Task<byte[]>>` — deferred, not a prebuilt `byte[]`. The generator runs only when output is written, so it can depend on late-stage state (the final search index, the resolved xref map) without blocking discovery. Return `Task.FromResult(bytes)` when the content is ready synchronously.
- `ContentType` is a string MIME. There is no enum. Common values: `text/plain`, `text/markdown`, `application/json`, `application/xml`, `image/png`.

The other members return empty — no routes for the crawler, no static files to copy, no sidebar rows, no xref ids:

- `DiscoverAsync` yields nothing.
- `GetContentToCopyAsync` returns `ImmutableList<ContentToCopy>.Empty`.
- `GetContentTocEntriesAsync` returns `ImmutableList<ContentTocItem>.Empty`.
- `GetCrossReferencesAsync` returns `ImmutableList<CrossReference>.Empty`.

`DefaultSectionLabel` and `SearchPriority` are read by consumers that group discovered items; since this service discovers nothing, they do not matter — return `""` and `0`.

## Register the implementation

`AddPennington` does not auto-discover `IContentService` implementations — register directly on `IServiceCollection`. Transient is the right lifetime for this shape: the service is stateless and the container can build a fresh instance per resolution.

```csharp
builder.Services.AddTransient<IContentService, RobotsTxtContentService>();
```

## Result

The static build writes a single file at the output root:

```text
User-agent: *
Allow: /
Sitemap: /sitemap.xml
```

Artifacts from `GetContentToCreateAsync` are emitted by the static build, not the live dev server. To serve the same bytes at request time during `dotnet run`, add a `MapGet("/robots.txt", ...)` endpoint that calls into the same generator — the `/llms.txt` endpoint wired by `AddLlmsTxt` is the reference for that pattern.

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample -- build output` and confirm `output/robots.txt` exists with the expected body.
- Fetch `/robots.txt` from the dev server and expect a 404 — the artifact is a build-time output, not a live route.

## Related

- Reference: [Content pipeline interfaces](xref:reference.api.i-content-service)
- How-to: [Source content from outside the file system](xref:how-to.extensibility.custom-content-service)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)

---
title: "Emit generated output artifacts"
description: "Implement an IContentService whose only job is to write a byte artifact to the output — robots.txt, search-index sidecars, social images — with no pages, TOC entries, or xrefs."
uid: how-to.content-services.emit-generated-artifacts
order: 208030
sectionLabel: "Content Services"
tags: [extensibility, content-service, artifacts]
---

To emit a byte artifact into the output — `robots.txt`, a sitemap variant, a social-image `.png`, a sidecar `.json` search index — that is not a routed page, not in navigation, and not an xref target, implement `IContentService` with `GetContentToCreateAsync` as the only meaningful member. Every other interface member returns empty. Artifacts emit during the static build only; the dev server returns 404 for them unless a sibling `MapGet` serves the same bytes at request time — the `/llms.txt` endpoint wired by `AddLlmsTxt` is the reference for that pattern.

For the opposite shape — a service that contributes routed pages, TOC entries, and xrefs from a non-markdown source — see <xref:how-to.content-services.custom-content-service>.

The recipe references `examples/ExtensibilityLabExample/RobotsTxtContentService.cs`. `LlmsTxtContentService` in the core library is the production example of the same pattern.

## Before you begin

- A working Pennington site on bare `AddPennington` (see <xref:tutorials.getting-started.first-site> if not).
- Familiarity with the four-stage pipeline at a conceptual level (<xref:explanation.core.content-pipeline>).

## Write the service

Implement <xref:reference.api.i-content-service> as a sealed class. Every member returns empty except `GetContentToCreateAsync`, which yields one `ContentToCreate` per artifact.

```csharp:path
examples/ExtensibilityLabExample/RobotsTxtContentService.cs
```

The three fields on `ContentToCreate` carry the surface:

- `OutputPath` is a `FilePath` relative to the output root. `new FilePath("robots.txt")` writes to `/robots.txt`; `new FilePath("assets/og/home.png")` writes to `/assets/og/home.png`.
- `ContentGenerator` is a `Func<Task<byte[]>>` — deferred, not a prebuilt `byte[]`. The generator runs only when output is written, so it can depend on late-stage state (the final search index, the resolved xref map) without blocking discovery. Return `Task.FromResult(bytes)` when the content is ready synchronously.
- `ContentType` is a MIME string. Common values: `text/plain`, `text/markdown`, `application/json`, `application/xml`, `image/png`.

## Register the service

`AddPennington` does not auto-discover `IContentService` implementations — register directly on `IServiceCollection`. The service is stateless, so transient works.

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

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample -- build output` and confirm `output/robots.txt` exists with the expected body.
- Fetch `/robots.txt` from the dev server and expect a 404 — the artifact is a build-time output, not a live route.

## Related

- Reference: [Content pipeline interfaces](xref:reference.api.i-content-service)
- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)

---
title: "Emit generated output artifacts"
description: "Implement an IArtifactContentService that owns a URL territory and produces byte artifacts — robots.txt, JSON sidecars, generated images — served live in dev and written into the static build."
uid: how-to.content-services.emit-generated-artifacts
order: 4
sectionLabel: "Content Services"
tags: [extensibility, artifacts]
---

To emit a byte artifact — `robots.txt`, a sitemap variant, a social-image `.png`, a sidecar `.json` index — that is not a routed page, not in navigation, and not an xref target, implement `IArtifactContentService`. The interface has three members and one rule: the same resolver produces the bytes for a live dev request and for the static build, so the two surfaces can never drift.

- `Claims` declares the URL territory the service owns (an exact path, a prefix, or a path suffix). Claims derive from options alone — they are consulted on every request and must never trigger expensive work.
- `ResolveAsync` turns one claimed path into bytes plus a content type, or returns null to decline so the request falls through to content routing.
- `DiscoverAsync` enumerates the routes the static build writes — each one resolved through `ResolveAsync` and written to its output file.

Pennington's own search shards (`/search/**.json`), llms.txt files, and book PDFs ship through this interface; `RobotsTxtContentService` below is the smallest possible example.

For the opposite case — a service that contributes routed pages, TOC entries, and xrefs from a non-markdown source — see <xref:how-to.content-services.custom-content-service>.

## Before you begin

- A working Pennington site on bare `AddPennington` (see <xref:tutorials.getting-started.first-site> if not).
- Familiarity with the four-stage pipeline at a conceptual level (<xref:explanation.core.content-pipeline>).

## Write the service

```csharp:symbol
examples/ExtensibilityLabExample/RobotsTxtContentService.cs
```

The pieces:

- `ArtifactClaim` carries an owner name, a shape, and a description. The shape is a union: `ExactClaim` (one path), `PrefixClaim` (everything under a prefix, optionally narrowed by extension — `/search/` + `.json`), or `SuffixClaim` (a path ending at any depth — this is how `{section}/llms.txt` works, a territory no endpoint route template can express). `diag routes` lists every registered claim.
- `ResolveAsync` receives the request path without its leading slash (`robots.txt`, `search/en/index.json`). Returning null declines: the request continues into content routing, so a real page under a claimed prefix keeps working.
- `DiscoverAsync` yields `DiscoveredItem`s with a `GeneratedSource`. Routes that should exist only in dev are resolvable without being enumerated — the book package serves its live `/book-preview/` this way while enumerating only the PDFs.
- The resolver may do expensive work on demand (build an index, fold over `ISiteProjection`, run a headless browser). The claims must not.

## Register the service

Register on the artifact tier — never as `IContentService`, which would put the service in every request-path discovery walk:

```csharp
builder.Services.AddTransient<IArtifactContentService, RobotsTxtContentService>();
```

## Result

The dev server answers `/robots.txt` live, and the static build writes the same bytes to the output root:

```text
User-agent: *
Allow: /
Sitemap: /sitemap.xml
```

## Verify

- Fetch `/robots.txt` from the dev server and expect the body above — same bytes both surfaces.
- Run `dotnet run --project examples/ExtensibilityLabExample -- build output` and confirm `output/robots.txt` exists with the expected body.
- Run `dotnet run -- diag routes` and confirm the claim appears under "Artifact territories".

## Related

- Reference: [Content pipeline interfaces](xref:reference.api.i-content-service)
- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)

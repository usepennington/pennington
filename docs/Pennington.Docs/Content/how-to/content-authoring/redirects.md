---
title: "Configure redirects"
description: "Set redirectUrl in DocSiteFrontMatter to emit a meta-refresh stub page that sends visitors to a new URL."
uid: how-to.content-authoring.redirects
order: 201120
sectionLabel: Content Authoring
tags: [redirects, front-matter, routing]
---

When you rename or delete a published page, the old URL needs to forward visitors and search engines to the new location. Setting `redirectUrl:` in the page's front matter causes Pennington to emit a meta-refresh stub at the old path — the body is not rendered or indexed.

This covers front-matter-based redirects only. HTTP 301 responses and batch redirects via a sidecar file are handled at the hosting layer and are outside this guide's scope.

## Assumptions

- You have an existing Pennington doc site using `AddDocSite` (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- You know the old URL (the page being retired) and the new URL (the canonical destination).
- Your front-matter type implements `IRedirectable` — `DocSiteFrontMatter` and `BlogSiteFrontMatter` both do.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/redirect-source.md` is the fixture this how-to is fenced from.

---

## Steps

### 1. Add `redirectUrl:` to the old page's front matter

Open the markdown file at the old URL and set `redirectUrl:` to the new absolute path. Keep `title:` so diagnostics remain readable; the body will not be rendered.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/redirect-source.md
```

### 2. Confirm your front-matter record implements `IRedirectable`

The engine looks for the `RedirectUrl` property via `IRedirectable` pattern-matching; `DocSiteFrontMatter` already declares it. If you use a custom front-matter record, add the interface so the pipeline will surface the page as a redirect.

```csharp:xmldocid
T:Pennington.DocSite.DocSiteFrontMatter
```

```csharp:xmldocid
T:Pennington.FrontMatter.IRedirectable
```

### 3. Understand what the pipeline emits

During discovery, `MarkdownContentService` detects `RedirectUrl` and yields a `RedirectSource` instead of a `MarkdownFileSource`, so the page skips parse/render and the redirect middleware handles it uniformly at dev serve and `build` time.

```csharp:xmldocid
M:Pennington.Content.MarkdownContentService`1.DiscoverAsync
```

### 4. Run the site and follow the old URL

Start the site with `dotnet run`. The old URL responds with a meta-refresh stub that forwards to `redirectUrl`; the static build writes the same stub to disk at the old page's output path.

```bash
dotnet run --project src/YourDocSite
```

---

## Verify

- Visit the old URL in a browser: expect an immediate redirect to the target set in `redirectUrl`.
- View the old URL's source: expect `<meta http-equiv="refresh" content="0;url=...">` and a `<link rel="canonical" href="...">` tag pointing at the redirect target.
- Check `/sitemap.xml` and `/llms.txt`: the old URL must not appear (redirects are filtered by `SitemapBuilder` and `LlmsTxtService`).

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys) — the row for `redirectUrl` (type, default, which records support it).
- Reference: [`IFrontMatter` and capability defaults](xref:reference.front-matter.ifrontmatter) — how `IRedirectable` fits alongside the other capability interfaces.
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities) — why `IRedirectable` stayed a separate capability instead of collapsing into `IFrontMatter`.

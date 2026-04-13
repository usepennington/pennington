---
title: "Configure redirects"
description: "Set redirectUrl in DocSiteFrontMatter to emit a meta-refresh stub page with noindex."
section: "content-authoring"
order: 100
tags: []
uid: how-to.content-authoring.redirects
isDraft: true
search: false
llms: false
---

> **In this page.** Setting `redirectUrl:` in `DocSiteFrontMatter` to emit a meta-refresh stub page with `noindex`.
>
> **Not in this page.** HTTP 301 responses at the hosting layer or batch redirects from a sidecar file.

## When to use this

- Outline (not prose): the reader has renamed or moved a page and wants incoming links to the old URL to land on the new one
- Aimed at authors who already publish with Pennington and need a per-page redirect that survives both `dotnet run` and `dotnet run -- build`
- Not a concept tour of `IRedirectable` or the content pipeline — both live in the explanation quadrant

## Assumptions

- You have an existing Pennington doc site wired via `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`
- Your front-matter type implements `IRedirectable` — `DocSiteFrontMatter` (the default for `AddDocSite`) already does, as do `BlogSiteFrontMatter` and the example records `MultipleContentSourceExample.DocsFrontMatter`, `UserInterfaceExample.DocsFrontMatter`, and `RoslynIntegrationExample.BlogFrontMatter`
- You know the canonical target URL the old page should redirect to (use site-relative paths such as `/getting-started/`)
- To copy a working setup, see `examples/BeaconDocsExample/` — it ships a redirect at `Content/setup.md` pointing to `/getting-started/`

```csharp:xmldocid
T:UserInterfaceExample.DocsFrontMatter
```

- Reference snippet: `UserInterfaceExample.DocsFrontMatter` — shows the minimal record shape that carries `RedirectUrl` alongside the other capability interfaces

---

## Steps

### 1. Identify the old URL you want to keep alive

- Keep the source markdown file at its existing path so its URL (the redirect source) is preserved
- Do not delete the file — the redirect is emitted from that file's front matter
- The target URL in `redirectUrl:` should be site-relative (leading `/`) and typically end with a trailing slash, matching canonical Pennington URLs

### 2. Replace the page body with front-matter-only redirect metadata

- Strip the file down to YAML front matter; the body is ignored once `redirectUrl` is set (the engine emits a stub, not your original HTML)
- The single required key beyond `title:` is `redirectUrl:`
- Optional: leave a one-line human-readable fallback link in the body for users whose browsers ignore meta refresh

```yaml
---
title: "Setup (Moved)"
description: "This page has moved"
redirectUrl: "/getting-started/"
---

This page has moved to [Getting Started](/getting-started/).
```

- Reference snippet: `examples/BeaconDocsExample/Content/setup.md` — verified redirect from `/setup/` to `/getting-started/` using `DocSiteFrontMatter`

### 3. (Alternative) Use a front-matter-only file with no title

- For purely internal redirects where no human will ever see the stub, a bare `redirectUrl:` is enough
- The engine still produces the meta-refresh stub; the file's URL is preserved as the redirect source

```yaml
---
redirectUrl: /sub-folder/page-one
---
```

- Reference snippet: `examples/MinimalExample/Content/sub-folder/page-1.md` — front-matter-only redirect file verified in the examples inventory

### 4. (If using a custom front-matter type) Implement `IRedirectable`

- If your site uses a custom front-matter record instead of `DocSiteFrontMatter`, add `IRedirectable` to the interface list and expose `string? RedirectUrl`
- No other wiring is needed — the pipeline discovers the capability by pattern-matching `IRedirectable`
- `DocSiteFrontMatter` and `BlogSiteFrontMatter` already implement it; only custom records need this step

```csharp:xmldocid
T:MultipleContentSourceExample.DocsFrontMatter
```

- Reference snippet: `MultipleContentSourceExample.DocsFrontMatter` — custom record implementing `IRedirectable` alongside `ITaggable` and `IOrderable`

### 5. Understand what the engine emits

- On `dotnet run -- build`, `OutputGenerationService` writes a stub HTML file at the old URL containing `<meta http-equiv="refresh" content="0;url={target}">` plus `<link rel="canonical" href="{target}">`
- The same source file is automatically excluded from the TOC, sidebar navigation, search index, `sitemap.xml`, and `llms.txt` (redirect pages carry no indexable content)
- On `dotnet run` (dev serve), the redirect is served by the same HTTP pipeline — the redirect stub is produced on request

---

## Verify

- Run `dotnet run --project <your-site>` and visit the old URL; confirm the browser follows the meta refresh to the target URL
- Run `dotnet run --project <your-site> -- build` and open `output/<old-path>/index.html`; confirm it contains `<meta http-equiv="refresh" content="0;url=/<target>/">`
- Open `output/sitemap.xml` and confirm the redirect URL is absent (redirects are filtered by `SitemapBuilder`)
- Open `output/search-index-<locale>.json` and confirm the redirect URL is absent (redirects are filtered by `MarkdownContentService.GetIndexableEntriesAsync`)

## Related

- Reference: Front matter keys table — covers `redirectUrl` alongside `title`, `description`, `tags`, `section`, `order`, etc.
- Reference: `IRedirectable` capability interface — `src/Pennington/FrontMatter/Capabilities.cs`
- Background: Explanation of the front-matter capability model and why redirects are transport-only metadata excluded from index surfaces

---
title: "Configure redirects"
description: "Set redirectUrl in DocSiteFrontMatter to emit a meta-refresh stub page that sends visitors to a new URL."
uid: how-to.content-authoring.redirects
order: 120
sectionLabel: Content Authoring
tags: [redirects, front-matter, routing]
---

> **In this page.** _Paraphrase the TOC "Covers" line: setting `redirectUrl:` in `DocSiteFrontMatter` so the page is emitted as a meta-refresh stub with `noindex`._
>
> **Not in this page.** _Paraphrase "Does not cover": HTTP 301 responses configured at the hosting layer, and batch redirects defined in a sidecar `_redirects.yml` file — link those out to Reference for `RedirectContentService` once that page exists (TODO: confirm target ref page)._

## When to use this

_Two sentences. Frame the reader's goal: they renamed or deleted a page, the old URL is already published, and they want visitors and search engines to land on the new location. Note that the page's body will not render — the output is a stub, so do not put content here you want indexed._

## Assumptions

_Three bullets max. Realistic prior state, not tutorial steps._

- You have an existing Pennington doc site using `AddDocSite` (see the [Getting Started tutorial](/tutorials/getting-started/minimal-site) if not).
- You know the old URL (the page being retired) and the new URL (the canonical destination).
- Your front-matter type implements `IRedirectable` — `DocSiteFrontMatter` and `BlogSiteFrontMatter` both do.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/redirect-source.md` is the fixture this how-to is fenced from.

---

## Steps

_Four steps. Each imperative, one sentence of prose max before the fence._

### 1. Add `redirectUrl:` to the old page's front matter

_Open the markdown file at the old URL and set `redirectUrl:` to the new absolute path. Keep `title:` so diagnostics remain readable; the body will not be rendered._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/redirect-source.md
```

### 2. Confirm your front-matter record implements `IRedirectable`

_The engine looks for the `RedirectUrl` property via `IRedirectable` pattern-matching; `DocSiteFrontMatter` already declares it. If you use a custom front-matter record, add the interface so the pipeline will surface the page as a redirect._

```csharp:xmldocid
T:Pennington.DocSite.DocSiteFrontMatter
```

```csharp:xmldocid
T:Pennington.FrontMatter.IRedirectable
```

### 3. Understand what the pipeline emits

_During discovery, `MarkdownContentService` detects `RedirectUrl` and yields a `RedirectSource` instead of a `MarkdownFileSource`, so the page skips parse/render and the redirect middleware handles it uniformly at dev serve and `build` time._

```csharp:xmldocid
M:Pennington.Content.MarkdownContentService`1.DiscoverAsync
```

### 4. Run the site and follow the old URL

_Start the site with `dotnet run`. The old URL responds with a meta-refresh stub that forwards to `redirectUrl`; the static build writes the same stub to disk at the old page's output path._

```bash
dotnet run --project src/YourDocSite
```

---

## Verify

- Visit the old URL in a browser: expect an immediate redirect to the target set in `redirectUrl`.
- View the old URL's source: expect `<meta http-equiv="refresh" ...>` and a `<meta name="robots" content="noindex">` tag (TODO: confirm `noindex` is emitted — TOC says yes, example page wording does not mention it).
- Check `/sitemap.xml` and `/llms.txt`: the old URL must not appear (redirects are filtered by `SitemapBuilder` and `LlmsTxtService`).

## Related

- Reference: [Front matter key reference](/reference/front-matter/keys) — the row for `redirectUrl` (type, default, which records support it).
- Reference: [`IFrontMatter` and capability defaults](/reference/front-matter/ifrontmatter) — how `IRedirectable` fits alongside the other capability interfaces.
- Background: [The front-matter capability system](/explanation/core/front-matter-capabilities) — why `IRedirectable` stayed a separate capability instead of collapsing into `IFrontMatter`.

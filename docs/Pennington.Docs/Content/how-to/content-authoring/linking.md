---
title: "Link between pages and assets"
description: "Write relative, absolute, fragment, and external links in markdown and let Pennington rewrite them for sub-path deployments."
uid: how-to.content-authoring.linking
order: 110
sectionLabel: Content Authoring
tags: [linking, routing, base-url, authoring]
---

> **In this page.** Relative links between pages, anchor fragments, external links, and how `BaseUrlHtmlRewriter` handles sub-path deployments.
>
> **Not in this page.** Cross-references by `uid:` — see the previous how-to. Programmatic URL construction lives in Reference → Routing.

## When to use this

_Two sentences. Name the trigger: you are authoring a page and want to link to another page, a heading on the current or a sibling page, an asset, or an external site. Point readers still wiring up their site back to the Getting Started tutorial so this page stays a recipe for authors, not a setup guide._

## Assumptions

_Short bulleted list. Keep prerequisites terse so the page reads as a recipe._

- You have an existing Pennington site with at least two markdown pages (see [_Add your first markdown page_](/tutorials/getting-started/first-page) if not).
- You know the URL of the page or asset you want to link to (the sidebar or the rendered page's address bar are the quickest sources).
- You are not using `uid:` cross-references — those have their own page at [_Cross-reference pages by `uid`_](/how-to/content-authoring/cross-references).

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample). The `Content/main/linking.md` fixture shows every link shape this page describes. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Link to a sibling page with a relative path

_One sentence: write a standard markdown link with a relative target like `./customize-sidebar`, and let `MarkdownLinkResolver` walk the content tree to resolve it against the current page's URL. Relative links survive section moves as long as the two files stay together, so prefer them for tightly coupled pages in the same folder._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/linking.md
```

### 2. Link to a page in another area with an absolute path

_One sentence: when the target lives in a different section or area (for example, an API reference page from a main-area guide), use a site-absolute path like `[API index](/api/)`. Absolute paths anchor at the site root, so they are stable across folder moves of the source page but will break if the target URL changes — reach for `uid:` cross-references when you need rename safety._

### 3. Jump to a heading with an anchor fragment

_One sentence: append `#slug` to a link target to scroll to a specific heading; headings are slugified by Markdig's auto-identifier pass so `## Relative links to sibling pages` becomes `#relative-links-to-sibling-pages`. The same fragment works inline (`[see below](#verify)`), on a relative link, and on a uid-based xref._

### 4. Link to colocated and shared assets

_One to two sentences: reference assets under `Content/` with a relative path (`./assets/colocated.png`) and assets under `wwwroot/` with a site-absolute path (`/shared.png`). The content copy pass and the static-file pipeline place the files at matching URLs, so the two rules map cleanly to where the file lives on disk. Point readers who need more background to [_Place images and static assets_](/how-to/content-authoring/images-and-assets)._

### 5. Link to an external site

_One sentence: write the full URL — `[Markdig](https://github.com/xoofx/markdig)` — and Pennington leaves the `href` untouched because only relative, root-relative, and uid-shaped links participate in rewriting. Add `rel="noopener"` or `target="_blank"` through a custom Markdig extension only when your hosting policy requires it; none of the built-in rewriters do this for you._

### 6. Deploy under a sub-path and let `BaseUrlHtmlRewriter` prepend the prefix

_Two sentences: set `OutputOptions.BaseUrl` (for example `/docs/`) so every rendered response gets `href`, `src`, and `action` attributes prefixed with the base URL at response time. Authors keep writing root-relative links like `/api/`, and the rewriter turns them into `/docs/api/` on the way out — do not hard-code the prefix in your markdown._

```csharp:xmldocid
T:Pennington.Infrastructure.BaseUrlHtmlRewriter
```

<!-- TODO confirm the xmldocid resolves — `T:Pennington.Infrastructure.BaseUrlHtmlRewriter` is listed in site-architecture.md and present at src/Pennington/Infrastructure/BaseUrlHtmlRewriter.cs. -->

---

## Verify

- Run `dotnet run` and click each link shape on `/main/linking/` — expect relative, absolute, anchor, asset, and external links to all navigate correctly.
- View source on the rendered page with `BaseUrl="/docs/"` — expect every internal `href` to start with `/docs/` and the `<body>` to carry a `data-base-url="/docs/"` attribute stamped by `BaseUrlHtmlRewriter`.
- Run `dotnet run -- build` — expect the build report to list zero broken-link diagnostics from `LinkVerificationService`.

## Related

- Reference: [_`OutputOptions`_](/reference/options/output-options) — `BaseUrl` and the rest of the build-output surface.
- Reference: [_Response rewriters_](/reference/infrastructure/response-rewriters) — rewriter order (`XrefHtmlRewriter` → `LocaleLinkHtmlRewriter` → `BaseUrlHtmlRewriter`) and how they compose.
- Background: [_How links are resolved_](/explanation/routing/link-resolution) — why authors write root-relative paths and the rewriter owns the transport-layer prefix.

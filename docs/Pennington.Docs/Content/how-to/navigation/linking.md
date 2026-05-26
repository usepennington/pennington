---
title: "Link between pages without hardcoding URLs"
description: "Pick the right link form for sibling pages, cross-area targets, anchors, assets, and external sites — and let Pennington rewrite for sub-path deployments."
uid: how-to.navigation.linking
order: 3
sectionLabel: "Navigation & Links"
tags: [linking, routing, base-url, authoring]
---

To link from one page to another without hardcoding a URL that may break on rename or sub-path deploy, pick the link form that matches the target's relationship to the source. For `uid:`-based cross-references that survive arbitrary page moves, see <xref:how-to.navigation.cross-references>.

## Before you begin
- An existing Pennington site with at least two markdown pages (see <xref:tutorials.getting-started.first-page> if not).
- The URL of the target page or asset is known (the sidebar or the rendered page's address bar are the quickest sources).

## Link forms

### Relative path to a sibling page

Write a standard markdown link with a relative target such as `[Customize the sidebar](./customize-sidebar)`. The target resolves against the current page's URL. Relative links survive section moves as long as both files stay in the same folder, which makes them the right choice for tightly coupled pages.

### Absolute path to a page in another area

When the target lives in a different section or area, use a site-absolute path: `[API index](/api/)`. Absolute paths are stable across folder moves of the source page but break if the target URL changes. Reach for `uid:` cross-references when rename safety matters.

### Anchor fragment to a heading

Append `#slug` to any link target to scroll to a specific heading. Markdig's auto-identifier pass slugifies headings, so `## Relative links to sibling pages` becomes `#relative-links-to-sibling-pages`. The same fragment syntax applies to relative links, absolute paths, and uid-based xrefs.

### Colocated and shared assets

Reference assets stored under `Content/` with a relative path (`./assets/diagram.png`) and assets under `wwwroot/` with a site-absolute path (`/shared.png`). The content copy pass and the static-file pipeline place the files at matching URLs, so the two rules map directly to where the file lives on disk. For more on asset placement, see <xref:how-to.pages.images-and-assets>.

### External site

Write the full URL directly: `[Markdig](https://github.com/xoofx/markdig)`. Pennington leaves the `href` untouched. For sites that need `rel="noopener"` or `target="_blank"` injected uniformly, write an `IHtmlResponseRewriter` (see <xref:how-to.response-pipeline.html-rewriter>).

### Sub-path deployment

Build with `--base-url /docs` so every rendered response has its `href`, `src`, and `action` attributes prefixed at response time. Write root-relative links like `/api/` in markdown — the rewriter turns them into `/docs/api/` on the way out.

```markdown
[API index](/api/)
```

Hardcoding the prefix in markdown defeats the rewriter. See <xref:how-to.deployment.base-url> for the build invocation.

## Verify

- Run `dotnet run` and click each link shape on `/main/linking/` — relative, absolute, anchor, asset, and external links all navigate correctly.
- View source on the rendered page with `BaseUrl="/docs/"` — every internal `href` starts with `/docs/` and the `<body>` carries a `data-base-url="/docs/"` attribute stamped by `BaseUrlHtmlRewriter`.
- Run `dotnet run -- build` — the build report lists zero broken-link diagnostics from `LinkVerificationService`.

## Related

- Reference: <xref:reference.api.output-options> — `BaseUrl` and the rest of the build-output surface.
- Reference: <xref:reference.api.i-response-processor> — rewriter order (`XrefHtmlRewriter` -> `LocaleLinkHtmlRewriter` -> `BaseUrlHtmlRewriter`) and how they compose.

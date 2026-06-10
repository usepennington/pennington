---
title: "Link between pages without hardcoding URLs"
description: "Pick the right link form for sibling pages, cross-area targets, anchors, assets, and external sites — and let Pennington rewrite for sub-path deployments."
uid: how-to.navigation.linking
order: 3
sectionLabel: "Navigation & Links"
tags: [linking, routing, base-url, authoring]
---

To link from one page to another without hardcoding a URL that may break on rename or sub-path deploy, pick the link form that matches the target's relationship to the source. When rename safety matters most, give the target a stable `uid:` and cross-reference it — the last form below.

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

### Cross-reference by uid

When the target may move to a different folder or get renamed, link by `uid:` instead of by path. Give the target page a stable, dot-separated `uid:` in its front matter — every shipped front-matter type already exposes the field through `IFrontMatter`, so filling it opts the page in.

```yaml
---
title: Configure the build pipeline
uid: how-to.build.configure
---
```

Then link to it with either form. The inline `<xref:uid>` defaults its link text to the target's `Title`:

```markdown
See <xref:kitchen-sink.main.cross-references-b> for the other half of this pairing.
```

The anchor-style `[text](xref:uid)` form takes a custom label:

```markdown
See the [cross-reference target page](xref:kitchen-sink.main.cross-references-b) for details.
```

Both resolve to an ordinary `<a href="/canonical/path">` at request and build time, so moving or renaming the target file leaves the link intact as long as its `uid:` does not change.

## Verify

- Run `dotnet run` and click each link shape on a page that uses them — relative, absolute, anchor, asset, and external links all navigate to the right target.
- Build for a sub-path with `dotnet run -- build --base-url /docs` and open any output page: every internal `href` starts with `/docs/` and the `<body>` carries a `data-base-url="/docs"` attribute (no trailing slash). Markdown that still hardcodes the prefix shows up as `/docs/docs/…`.
- Break a `uid:` on purpose — the `xref:` link renders with `data-xref-error="Reference not found"`, a warning appears in the dev diagnostic overlay, and `dotnet run -- build` surfaces it in the `BuildReport`.
- Run `dotnet run -- build` — the build report lists zero broken-link diagnostics.

## Related

- Reference: <xref:reference.api.output-options> — `BaseUrl` and the rest of the build-output surface.
- Reference: <xref:reference.api.i-response-processor> — the response-stage rewriters and how they compose.
- Background: <xref:explanation.routing.cross-references> — the two-phase uid resolver, ordering, and diagnostics.

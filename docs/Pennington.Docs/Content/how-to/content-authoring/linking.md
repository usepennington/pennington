---
title: "Link between pages and assets"
description: "Write relative, absolute, fragment, and external links in markdown and let Pennington rewrite them for sub-path deployments."
uid: how-to.content-authoring.linking
order: 201110
sectionLabel: Content Authoring
tags: [linking, routing, base-url, authoring]
---

When authoring a page that needs to link to another page, a heading anchor, a colocated asset, or an external site, use the patterns on this page. For `uid:`-based cross-references that survive page renames, see <xref:how-to.content-authoring.cross-references> instead.

## Assumptions

- An existing Pennington site with at least two markdown pages (see <xref:tutorials.getting-started.first-page> if not).
- The URL of the target page or asset is known (the sidebar or the rendered page's address bar are the quickest sources).

---

## Steps

<Steps>
<Step StepNumber="1">

**Link to a sibling page with a relative path**

Write a standard markdown link with a relative target such as `[Customize the sidebar](./customize-sidebar)`. `MarkdownLinkResolver` walks the content tree and resolves the target against the current page's URL. Relative links survive section moves as long as both files stay in the same folder, which makes them the right choice for tightly coupled pages.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/linking.md
```

</Step>
<Step StepNumber="2">

**Link to a page in another area with an absolute path**

When the target lives in a different section or area, use a site-absolute path: `[API index](/api/)`. Absolute paths are stable across folder moves of the source page but break if the target URL changes. Reach for `uid:` cross-references when rename safety matters.

</Step>
<Step StepNumber="3">

**Jump to a heading with an anchor fragment**

Append `#slug` to any link target to scroll to a specific heading. Markdig's auto-identifier pass slugifies headings, so `## Relative links to sibling pages` becomes `#relative-links-to-sibling-pages`. The same fragment syntax applies to relative links, absolute paths, and uid-based xrefs.

</Step>
<Step StepNumber="4">

**Link to colocated and shared assets**

Reference assets stored under `Content/` with a relative path (`./assets/diagram.png`) and assets under `wwwroot/` with a site-absolute path (`/shared.png`). The content copy pass and the static-file pipeline place the files at matching URLs, so the two rules map directly to where the file lives on disk. For more on asset placement, see <xref:how-to.content-authoring.images-and-assets>.

</Step>
<Step StepNumber="5">

**Link to an external site**

Write the full URL directly: `[Markdig](https://github.com/xoofx/markdig)`. Pennington leaves the `href` untouched — only relative, root-relative, and uid-shaped links participate in rewriting. Add `rel="noopener"` or `target="_blank"` through a custom Markdig extension when a hosting policy requires it; none of the built-in rewriters add these attributes.

</Step>
<Step StepNumber="6">

**Deploy under a sub-path and let `BaseUrlHtmlRewriter` prepend the prefix**

Set `OutputOptions.BaseUrl` (for example `/docs/`) so every rendered response has its `href`, `src`, and `action` attributes prefixed at response time. Write root-relative links like `/api/` in markdown, and the rewriter turns them into `/docs/api/` on the way out. Avoid hard-coding the prefix in markdown.

```csharp:xmldocid
T:Pennington.Infrastructure.BaseUrlHtmlRewriter
```

<!-- TODO confirm the xmldocid resolves — `T:Pennington.Infrastructure.BaseUrlHtmlRewriter` is listed in site-architecture.md and present at src/Pennington/Infrastructure/BaseUrlHtmlRewriter.cs. -->

</Step>
</Steps>

---

## Verify

- Run `dotnet run` and click each link shape on `/main/linking/` — relative, absolute, anchor, asset, and external links all navigate correctly.
- View source on the rendered page with `BaseUrl="/docs/"` — every internal `href` starts with `/docs/` and the `<body>` carries a `data-base-url="/docs/"` attribute stamped by `BaseUrlHtmlRewriter`.
- Run `dotnet run -- build` — the build report lists zero broken-link diagnostics from `LinkVerificationService`.

## Related

- Reference: [_`OutputOptions`_](xref:reference.options.auxiliary-options) — `BaseUrl` and the rest of the build-output surface.
- Reference: [_Response rewriters_](xref:reference.extension-points.response-processing) — rewriter order (`XrefHtmlRewriter` → `LocaleLinkHtmlRewriter` → `BaseUrlHtmlRewriter`) and how they compose.

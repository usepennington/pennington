---
title: Link between pages and assets
description: Relative links, anchor fragments, external links, and how `BaseUrlHtmlRewriter` handles sub-path deployments.
section: content-authoring
order: 110
tags: []
uid: how-to.content-authoring.linking
isDraft: true
search: false
llms: false
---

> **In this page.** Relative links, anchor fragments, external links, and how `BaseUrlHtmlRewriter` handles sub-path deployments.
>
> **Not in this page.** Cross-references by `uid` (see [cross-references](/how-to/content-authoring/cross-references)) or programmatic URL construction (see Reference → Routing).

## When to use this

When you are authoring markdown in an existing Pennington site and need to link to a sibling page, drop in an image, add a jump link, or link externally — especially if you plan to deploy the site under a sub-path and want links to survive the prefix.

## Assumptions

- You have a working Pennington site with at least one `AddMarkdownContent<T>` registration.
- You are editing `.md` files under a configured `ContentPath`.
- You understand front matter basics (see [Work with front matter](/how-to/content-authoring/front-matter)).

To copy a working setup, see [`examples/MinimalExample`](https://github.com/usepennington/pennington/tree/main/examples/MinimalExample) (sibling markdown plus image assets) and [`examples/BeaconDocsExample`](https://github.com/usepennington/pennington/tree/main/examples/BeaconDocsExample) (multi-section docs site).

---

## Steps

### 1. Link to another markdown page with a relative path

Write the href as a filesystem-relative path from the current `.md` file; it rewrites to the target's canonical URL at render time.

- `[Install](../getting-started/install.md)` — the `.md` suffix is stripped.
- `[Install](../getting-started/install)` (no extension) also works — `.md`, `.markdown`, and `.mdx` are all tried.
- `[Next page](sample-post)` resolves a bare sibling name against the current file's directory.
- If the target is not discovered, the href is left unchanged.

### 2. Append an anchor fragment or query string

Fragments and query strings pass through unchanged after the path is rewritten.

- `[See logging section](./configuration.md#logging)` — rewritten path plus `#logging`.
- `[Filtered view](./index.md?tag=rss)` — rewritten path plus `?tag=rss`.
- Pure-fragment links (`[Back to top](#top)`) are left untouched.

### 3. Reference an image or other asset relative to the markdown file

Place assets next to the content and reference them relatively. They resolve against the owning content source's `BasePageUrl`.

- `![Diagram](./images/pipeline.png)` — an absolute URL rooted at the content source (for example `/images/pipeline.png`).
- Works for any non-markdown asset (png, svg, pdf, zip) served from the same `ContentPath`.
- The asset must live inside a registered `ContentPath` tree; otherwise the href is left unchanged.

### 4. Link externally

Absolute and external hrefs pass through untouched.

- `[Anthropic](https://www.anthropic.com)` — untouched.
- `[Call support](tel:+15555550100)`, `[Email](mailto:hello@example.com)` — untouched.
- `[Protocol-relative](//cdn.example.com/x.js)` — untouched.
- Root-relative paths (`/styles.css`) bypass markdown link resolution but are still subject to `BaseUrlHtmlRewriter` in step 6.

### 5. Jump to a heading on the current page

Heading IDs are auto-generated from the heading text in kebab-case; link to them with a pure fragment.

```markdown
## Setup steps

See the [setup steps](#setup-steps) above.
```

### 6. Deploy under a sub-path

Pass the base URL to the `build` verb; `BaseUrlHtmlRewriter` prefixes every root-relative `href`, `src`, and `action` at response time. It also stamps `<body data-base-url="/preview">` so client-side scripts can prefix dynamically constructed URLs.

```bash
dotnet run --project src/MySite -- build /preview ./output
```

The rewriter skips empty or `/` base URLs, so dev serve is unaffected.

---

## Verify

- Run `dotnet run --project src/MySite` and hover rewritten links — hrefs should point at canonical URLs, not `.md` paths.
- Run `dotnet run --project src/MySite -- build /preview ./output` and grep the generated HTML for `href="/preview/` on previously root-relative links, plus `data-base-url="/preview"` on `<body>`.
- Links with `#fragment` or `?query` still carry their tail after rewriting.

## Related

- Reference: [Routing types](/reference/extension-points/routing)
- Reference: [Response processing interfaces](/reference/extension-points/response-processing)
- Background: [URL paths and content routes](/explanation/routing/url-paths)

---
title: Link between pages and assets
description: Relative links, anchor fragments, external links, and how BaseUrlHtmlRewriter handles sub-path deployments.
section: content-authoring
order: 90
tags: []
uid: how-to.content-authoring.linking
isDraft: true
search: false
llms: false
---

> **In this page.** Relative links, anchor fragments, external links, and how `BaseUrlHtmlRewriter` handles sub-path deployments.
>
> **Not in this page.** Cross-references by uid (see previous how-to on xrefs) or programmatic URL construction (see Reference -> Routing).

## When to use this

- You are authoring markdown in an existing Pennington site and need to link to a sibling page, drop in an image, add a jump link, or link externally.
- You are preparing to deploy the site under a sub-path (for example `/preview/`) and want links to survive the prefix.

## Assumptions

- You have a working Pennington site with at least one `AddMarkdownContent<T>` registration.
- You are editing `.md` files under a configured `ContentPath`.
- You understand front matter basics (see [front-matter](/how-to/content-authoring/front-matter)).

To copy a working setup, see [`examples/MinimalExample`](https://github.com/phil-scott-78/Pennington/tree/main/examples/MinimalExample) (sibling markdown + image assets) and [`examples/BeaconDocsExample`](https://github.com/phil-scott-78/Pennington/tree/main/examples/BeaconDocsExample) (multi-section docs site). Do not walk through the examples — this page is a recipe.

---

## Steps

### 1. Link to another markdown page with a relative path

Write the href as a filesystem-relative path from the current `.md` file. `MarkdownLinkResolver` rewrites it to the target's canonical URL at render time.

- `[Install](../getting-started/install.md)` -> canonical URL of that page (the `.md` suffix is stripped).
- `[Install](../getting-started/install)` (no extension) also works — the resolver tries `.md`, `.markdown`, `.mdx`.
- `[Next page](sample-post)` resolves a bare sibling name against the current file's directory.
- If the target is not discovered, the href is left unchanged.

### 2. Append an anchor fragment or query string

Fragments and query strings pass through unchanged after the path is rewritten.

- `[See logging section](./configuration.md#logging)` -> rewritten path + `#logging`.
- `[Filtered view](./index.md?tag=rss)` -> rewritten path + `?tag=rss`.
- Pure-fragment links (`[Back to top](#top)`) are left untouched — they always refer to the current page.

### 3. Reference an image or other asset relative to the markdown file

Place assets next to the content and reference them relatively. `MarkdownLinkResolver` resolves them against the owning content source's `BasePageUrl`.

- `![Diagram](./images/pipeline.png)` -> absolute URL rooted at the content source (for example `/images/pipeline.png`).
- Works for any non-markdown asset (png, svg, pdf, zip) served from the same `ContentPath`.
- The asset must live inside a registered `MarkdownContentOptions.ContentPath` tree; otherwise the href is left unchanged.

### 4. Link externally

Leave absolute and external hrefs alone — the resolver explicitly skips them.

- `[Anthropic](https://www.anthropic.com)` — untouched.
- `[Call support](tel:+15555550100)`, `[Email](mailto:hello@example.com)` — untouched.
- `[Protocol-relative](//cdn.example.com/x.js)` — untouched.
- Root-relative paths (`/styles.css`) bypass the markdown resolver but are still subject to `BaseUrlHtmlRewriter` in step 6.

### 5. Jump to a heading on the current page

Markdig's auto-identifier extension stamps heading IDs from the heading text; link to them with a pure fragment.

```markdown
## Setup steps

See the [setup steps](#setup-steps) above.
```

- Fragments use kebab-case derived from the heading text.
- Pure-fragment hrefs are the only relative-looking hrefs the resolver intentionally skips.

### 6. Deploy under a sub-path

Pass the base URL to the `build` verb; `BaseUrlHtmlRewriter` prefixes every root-relative `href`, `src`, and `action` at response time.

```bash
dotnet run --project src/MySite -- build /preview ./output
```

- `OutputOptions.FromArgs` reads `args[0] == "build"`, `args[1]` as `BaseUrl`, `args[2]` as `OutputDirectory`.
- `BaseUrlHtmlRewriter` (Order 30, last rewriter in the HTML pass) prefixes attributes on `[href]`, `[src]`, `[action]` when the value starts with a single `/` (not `//`).
- The rewriter stamps `<body data-base-url="/preview">` so client-side scripts (for example the search widget reading `document.body.getAttribute('data-base-url')`) prefix dynamically constructed URLs.
- Skips rewriting when `BaseUrl` is empty or `/`, so dev serve is unaffected.

---

## Verify

- Run `dotnet run --project src/MySite` and load a rewritten page — hover rewritten links and confirm hrefs point at canonical URLs, not `.md` paths.
- Run `dotnet run --project src/MySite -- build /preview ./output` and grep the generated HTML for `href="/preview/` on previously root-relative links, plus `data-base-url="/preview"` on `<body>`.
- Links with `#fragment` or `?query` still carry their tail after rewriting.

## Related

- Reference: [Routing types and URL helpers](/reference/routing)
- Reference: [Response rewriters and processor pipeline](/reference/infrastructure/response-processors)
- Background: [How URL rewriting is layered (xref -> locale -> base URL)](/explanation/url-rewriting-pipeline)

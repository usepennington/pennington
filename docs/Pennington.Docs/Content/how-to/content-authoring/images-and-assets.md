---
title: "Place images and static assets"
description: "Where to put images next to content, how to reference them from markdown, and how they reach the published output."
section: content-authoring
order: 40
tags: []
uid: how-to.content-authoring.images-and-assets
isDraft: true
search: false
llms: false
---

> **In this page.** Placing image and binary assets, referencing them from markdown, the two directory patterns (`Content/`-colocated vs `wwwroot/`-shared), and how each lands in `output/`.
>
> **Not in this page.** Image optimization pipelines, responsive-image helpers, or CDN wiring — none of those are built-in.

## When to use this

- You have screenshots, diagrams, PDF downloads, or other binary assets to include alongside markdown content.
- You want the asset to survive `dotnet run -- build` and land in the generated `output/` tree.

## Assumptions

- A working Pennington site with at least one `AddMarkdownContent<T>(...)` registration.
- You are comfortable editing `.md` files and dropping files into the project directory.
- You have read step 3 of [linking](/how-to/content-authoring/linking) — that's the resolver semantics this page builds on.

To copy a working setup, see [`examples/MinimalExample`](https://github.com/usepennington/pennington/tree/main/examples/MinimalExample). It ships `Content/media/sample.svg` (a shared asset inside the content tree) and `Content/sub-folder/sibling-sample.svg` (a colocated sibling asset), and `sample-post.md` references both. Do not walk the example — this page is a recipe.

---

## Steps

### 1. Decide where the asset belongs

Two placements, two resolver paths:

- **Colocated inside the content tree** — put the file next to the markdown page that uses it (under a registered `MarkdownContentOptions.ContentPath`). Use this for page-scoped assets (diagrams tied to one page, screenshots for one how-to).
- **Shared under `wwwroot/`** — put the file in the project's `wwwroot/` tree. Use this for assets referenced by many pages (logos, favicons, shared PDFs).

The resolver rules are covered in [linking, step 3](/how-to/content-authoring/linking) — this page uses those rules without re-deriving them.

### 2. Place the file in `Content/`-colocated form

Create an `images/` subdirectory next to the markdown file and drop the asset in. Any folder name works — `images/`, `media/`, `assets/`, flat next to the `.md` — but pick a convention and stick to it.

```text
Content/
  guides/
    my-page.md
    images/
      diagram.png
```

- Files inside a registered `ContentPath` are copied into the output tree alongside the rendered HTML.
- Binary extensions (png, svg, jpg, pdf, zip) pass through untouched. Markdown files (`.md`, `.markdown`, `.mdx`) are rendered, not copied.

### 3. Reference it from markdown

Write the `src` as a path relative to the current `.md` file. `MarkdownLinkResolver` rewrites it to an absolute URL rooted at the owning content source's `BasePageUrl`.

```markdown
![Architecture diagram](./images/diagram.png)
```

- The `./` is optional — `images/diagram.png` resolves identically.
- External URLs (`https://…`), protocol-relative URLs (`//cdn.example.com/x.png`), and pure fragments (`#top`) pass through unchanged.
- If the asset is not discovered inside any registered `ContentPath`, the href is left unchanged and you will see a broken image at runtime.

### 4. Place shared assets in `wwwroot/`

Drop the asset in the project's `wwwroot/` tree. ASP.NET's static-file middleware serves it directly at the matching URL — Pennington does not need to know about it.

```text
wwwroot/
  logo.svg
```

Reference from markdown with a root-relative path:

```markdown
![Project logo](/logo.svg)
```

- Root-relative paths (`/…`) bypass the markdown resolver entirely.
- Under a sub-path deployment, `BaseUrlHtmlRewriter` prefixes these at response time (see [linking, step 6](/how-to/content-authoring/linking)).

### 5. Verify asset copying on build

Run the build and spot-check the output tree.

```bash
dotnet run --project src/MySite -- build
```

- Colocated asset: expect `output/guides/images/diagram.png` (under the content source's `BasePageUrl`).
- Shared asset: expect `output/logo.svg` at the root of the output tree.
- Broken image references surface in the `BuildReport` alongside broken link diagnostics when link verification runs.

---

## Verify

- Run `dotnet run --project src/MySite`, load the page, and confirm the image renders at the rewritten URL.
- Run `dotnet run --project src/MySite -- build` and confirm the asset lands in `output/` at the expected path.
- The `BuildReport` lists no broken image references for the page.

## Related

- How-to: [Link between pages and assets](/how-to/content-authoring/linking)
- Reference: [`MarkdownContentOptions`](/reference/options/markdown-content-options)
- Reference: [`BuildReport` diagnostics](/reference/diagnostics/build-report)

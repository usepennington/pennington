---
title: "Place images and static assets"
description: "Colocate images with markdown or share them from wwwroot, and reference each from a page."
uid: how-to.content-authoring.images-and-assets
order: 201040
sectionLabel: Content Authoring
tags: [images, assets, wwwroot, authoring]
---

> **In this page.** Where image files live next to content, how to reference them relatively from markdown, and how they land in the published output — both `Content/`-colocated assets and `wwwroot/` shared assets.
>
> **Not in this page.** Image optimization pipelines and responsive-image helpers are out of scope. See the Explanation quadrant when it lands for discussion of why optimization is opt-in.

## When to use this

_Two sentences. Cover the two triggers: you want to embed an image that belongs to exactly one page (colocate under `Content/`), or you want a file referenced from many pages — a logo, a cover photo, a font — which belongs in `wwwroot/`. Point readers who are still setting up a site back to the Getting Started tutorial._

## Assumptions

_Short bulleted list. Do not re-teach setup._

- You have an existing Pennington site with at least one markdown page (see [_Add your first markdown page_](xref:tutorials.getting-started.first-page) if not).
- You know which markdown file you want the image to appear on.
- Your project has a `wwwroot/` folder for shared static files (the default for Web SDK projects).

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Colocate an image next to its markdown file

_One sentence: drop the image into a folder alongside the page, typically an `assets/` subfolder. Pennington's `MarkdownContentService.GetContentToCopyAsync` walks the content tree and copies every non-markdown file to the same relative path in the output, so the image ships with the page. Show the raw markdown page that references a colocated image with a relative `./assets/...` link._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/images-and-assets.md
```

### 2. Reference the colocated image with a relative path

_One sentence: use a standard markdown image with a relative path like `./assets/colocated.png`. Pennington's `MarkdownLinkResolver` resolves the link against the source file's URL so it renders correctly whether the page is served at `/main/images-and-assets/` or under a locale prefix._

### 3. Put shared assets in `wwwroot/`

_One sentence: when the same image is referenced from multiple pages — a site logo, a cover photo, a social-card image — drop it into `wwwroot/` instead so it has one canonical URL. Pennington wires `UseStaticFiles` for `wwwroot/` inside `UsePennington`, so the file is served at the matching path (e.g. `wwwroot/shared.png` → `/shared.png`)._

### 4. Reference the shared asset with an absolute path

_One sentence: write the markdown image with a leading slash — `![alt](/shared.png)` — so it resolves at the site root regardless of which page embeds it. If you deploy under a sub-path, the `BaseUrlHtmlRewriter` prepends the base URL at response time; do not hard-code the sub-path._

### 5. (Optional) Exclude an asset subtree from the copy pass

_One sentence: if a folder under `Content/` is owned by a different content service — or should not ship at all — list it in `MarkdownContentServiceOptions.ExcludePaths` when you call `AddMarkdownContent<T>`. The copy pass skips any relative path matched by `ExcludePaths`._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

<!-- TODO verify whether any symbol on BuildDocSiteOptions demonstrates ExcludePaths; if not, drop step 5 or replace with a plain YAML/config fence. -->

---

## Verify

- Run `dotnet run` and open the page that references the colocated image — expect the image to render inline.
- Visit the shared-asset URL directly (e.g. `/shared.png`) — expect the file to load.
- Run `dotnet run -- build` and confirm both files land in `output/` at their original paths (`output/main/assets/colocated.png`, `output/shared.png`).

## Related

- Reference: [_`MarkdownContentServiceOptions`_](xref:reference.options.markdown-content-options) — `ContentPath`, `ExcludePaths`, `FilePattern`.
- Reference: [_Front matter key reference_](xref:reference.front-matter.keys) — when an image is set as a social card or cover image via front matter.
- Background: [_How Pennington builds the output directory_](xref:explanation.core.dev-vs-build) — how `GetContentToCopyAsync` fits the content pipeline.

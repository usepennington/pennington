---
title: "Place images alongside the markdown that uses them"
description: "Colocate page-specific images next to their markdown, and put shared images in wwwroot for one canonical URL."
uid: how-to.content-authoring.images-and-assets
order: 201040
sectionLabel: Content Authoring
tags: [images, assets, wwwroot, authoring]
---

To keep an image next to the markdown that references it, drop it into a folder alongside the page under `Content/`. When the same file is needed from multiple pages — a logo, a cover photo, a shared diagram — put it in `wwwroot/` instead so it has one canonical URL.

## Assumptions

- An existing Pennington site with at least one markdown page (see <xref:tutorials.getting-started.first-page> if not).
- The target markdown file is known.
- The project has a `wwwroot/` folder for shared static files (the default for Web SDK projects).

## Where to put images

### Colocated next to the markdown file

Drop the image into a folder alongside the page — typically an `assets/` subfolder. `MarkdownContentService` walks the content tree and copies every non-markdown file to the same relative path in the output, so the image ships with the page automatically.

Reference it with a standard markdown image tag and a relative path:

```markdown
![Alt text](./assets/colocated.png)
```

`MarkdownLinkResolver` resolves the link against the source file's URL, so it renders correctly whether the page is served at `/main/images-and-assets/` or under a locale prefix.

### Shared in `wwwroot/`

When the same image is referenced from multiple pages, drop it into `wwwroot/` so it has one canonical URL. `UsePennington` wires `UseStaticFiles` for `wwwroot/`, so `wwwroot/shared.png` is served at `/shared.png`.

Reference it with a leading slash so it resolves at the site root regardless of which page embeds it:

```markdown
![Alt text](/shared.png)
```

When deploying under a sub-path, `BaseUrlHtmlRewriter` prepends the base URL at response time — avoid hard-coding the sub-path.

### Excluded subtrees

To keep a folder under `Content/` out of the output, list it in `MarkdownContentOptions.ExcludePaths` when calling `AddMarkdownContent<T>`. The copy pass skips any relative path matched by `ExcludePaths`.

```csharp:xmldocid
P:Pennington.Infrastructure.MarkdownContentOptions.ExcludePaths
```

## Verify

- Run `dotnet run` and open the page that references the colocated image — the image renders inline.
- Visit the shared-asset URL directly (for example, `/shared.png`) — the file loads.
- Run `dotnet run -- build` and confirm both files appear in `output/` at their original relative paths.

## Related

- <xref:reference.api.markdown-content-options> — `ContentPath`, `ExcludePaths`, `FilePattern`.
- <xref:reference.front-matter.keys> — when an image is set as a social card or cover image via front matter.
- <xref:explanation.core.dev-vs-build> — how `GetContentToCopyAsync` fits the content pipeline.

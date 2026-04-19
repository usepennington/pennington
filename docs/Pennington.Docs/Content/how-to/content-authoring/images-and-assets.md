---
title: "Place images and static assets"
description: "Colocate images with markdown or share them from wwwroot, and reference each from a page."
uid: how-to.content-authoring.images-and-assets
order: 201040
sectionLabel: Content Authoring
tags: [images, assets, wwwroot, authoring]
---

To embed an image that belongs to exactly one page, colocate it under `Content/` alongside its markdown file. When the same file is needed on multiple pages (a logo, a cover photo, a shared diagram), put it in `wwwroot/` instead. For a starting point, see <xref:tutorials.getting-started.first-page>.

## Assumptions

- An existing Pennington site with at least one markdown page.
- The target markdown file is known.
- The project has a `wwwroot/` folder for shared static files (the default for Web SDK projects).

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample).

---

## Steps

<Steps>
<Step StepNumber="1">

**Colocate an image next to its markdown file**

Drop the image into a folder alongside the page — typically an `assets/` subfolder. `MarkdownContentService` walks the content tree and copies every non-markdown file to the same relative path in the output, so the image ships with the page automatically.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/images-and-assets.md
```

</Step>
<Step StepNumber="2">

**Reference the colocated image with a relative path**

Use a standard markdown image tag with a relative path:

```markdown
![Alt text](./assets/colocated.png)
```

`MarkdownLinkResolver` resolves the link against the source file's URL, so it renders correctly whether the page is served at `/main/images-and-assets/` or under a locale prefix.

</Step>
<Step StepNumber="3">

**Put shared assets in `wwwroot/`**

When the same image is referenced from multiple pages, drop it into `wwwroot/` so it has one canonical URL. `UsePennington` wires `UseStaticFiles` for `wwwroot/`, so `wwwroot/shared.png` is served at `/shared.png`.

</Step>
<Step StepNumber="4">

**Reference the shared asset with an absolute path**

Write the markdown image with a leading slash so it resolves at the site root regardless of which page embeds it:

```markdown
![Alt text](/shared.png)
```

When deploying under a sub-path, `BaseUrlHtmlRewriter` prepends the base URL at response time — avoid hard-coding the sub-path.

</Step>
<Step StepNumber="5">

**(Optional) Exclude an asset subtree from the copy pass**

To keep a folder under `Content/` out of the output, list it in `MarkdownContentServiceOptions.ExcludePaths` when calling `AddMarkdownContent<T>`. The copy pass skips any relative path matched by `ExcludePaths`.

<!-- TODO: xmldocid needed -->

</Step>
</Steps>

---

## Verify

- Run `dotnet run` and open the page that references the colocated image — the image renders inline.
- Visit the shared-asset URL directly (for example, `/shared.png`) — the file loads.
- Run `dotnet run -- build` and confirm both files appear in `output/` at their original relative paths.

## Related

- <xref:reference.api.markdown-content-options> — `ContentPath`, `ExcludePaths`, `FilePattern`.
- <xref:reference.front-matter.keys> — when an image is set as a social card or cover image via front matter.
- <xref:explanation.core.dev-vs-build> — how `GetContentToCopyAsync` fits the content pipeline.

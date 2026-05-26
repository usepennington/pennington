---
title: "Reuse one snippet across many pages"
description: "Pull a shared Markdown partial into a page with the DocFX-style [!INCLUDE] directive — block or inline — instead of copy-pasting."
uid: how-to.pages.include-shared-content
order: 5
sectionLabel: "Pages"
tags: [authoring, markdown, includes, reuse]
---

When the same prerequisite note, install snippet, or boilerplate paragraph belongs on several pages, write it once and pull it in with an `[!INCLUDE]` directive. Pennington resolves the directive while parsing — the host page renders as if the partial's text were typed inline.

## Before you begin
- An existing Pennington site renders Markdown (see <xref:tutorials.getting-started.first-site> if not).
- A folder for partials that sits **outside** any content root, so the partials are not discovered as standalone pages. This site keeps them in `_includes/` next to `Content/`.
- The pipeline was built through `AddPennington` / `AddDocSite` / `AddBlogSite`; include expansion runs in the Markdown parser with no extra wiring.

## Block include

To drop a whole partial in as its own block — paragraphs, lists, callouts, code — put the directive on its own line. The path is relative to the page that references it.

````markdown
[!INCLUDE [.NET prerequisite](../../../_includes/dotnet-preview-note.md)]
````

The partial `_includes/dotnet-preview-note.md` holds a single `[!NOTE]` callout. It expands here, and the alert syntax inside it passes through the normal pipeline:

[!INCLUDE [.NET prerequisite](../../../_includes/dotnet-preview-note.md)]

The same partial expands the same way on every page that references it — edit `_includes/dotnet-preview-note.md` once and every host page picks up the change on the next build.

## Inline include

To splice a partial into the middle of a sentence, put the directive inline. The partial should hold a single run of text with no trailing newline.

````markdown
The current target SDK is [!INCLUDE [version](../../../_includes/sdk-version.md)], a preview build.
````

The current target SDK is [!INCLUDE [version](../../../_includes/sdk-version.md)], a preview build.

## How paths resolve

The path in the directive is resolved relative to the **referencing file**, not the content root — `../` climbs out of the page's folder. Includes nest: a partial may itself contain `[!INCLUDE]` directives, resolved relative to that partial in turn.

Two rules keep the feature predictable:

- A directive inside a fenced code block is left verbatim, so this page can show the syntax without expanding it.
- Relative links and images inside a partial are **not** rebased — they resolve as if written in the host page. Keep partials free of relative links, or use site-absolute paths.

## When a partial is missing

A target that does not exist, or an include cycle, collapses to an HTML comment instead of failing the build:

```html
<!-- Pennington: include not found: ../../../_includes/typo.md -->
```

View source on a page during `dotnet run` to spot a mistyped path — the comment names the directive that could not be resolved.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — include directives alongside every other non-CommonMark feature
- How-to: <xref:how-to.rich-content.content-tabs> — tab a partial's worth of content per platform or language
- How-to: <xref:how-to.pages.front-matter> — the front-matter keys a host page still needs (a partial's own front matter is stripped on include)

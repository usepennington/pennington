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

The partial `_includes/dotnet-preview-note.md` holds a single `[!NOTE]` callout. It expands here, and the alert syntax inside it passes through the normal pipeline — note that the partial's leading YAML front matter, if any, is stripped on include:

[!INCLUDE [.NET prerequisite](../../../_includes/dotnet-preview-note.md)]

The same partial expands the same way on every page that references it — edit `_includes/dotnet-preview-note.md` once and every host page picks up the change on the next build.

## Inline include

To splice a partial into the middle of a sentence, put the directive inline. The partial should hold a single run of text with no trailing newline.

````markdown
The current target SDK is [!INCLUDE [version](../../../_includes/sdk-version.md)], the stable release.
````

The current target SDK is [!INCLUDE [version](../../../_includes/sdk-version.md)], the stable release.

## How paths resolve

The path in the directive is resolved relative to the **referencing file**, not the content root — `../` climbs out of the page's folder. Includes nest: a partial may itself contain `[!INCLUDE]` directives, resolved relative to that partial in turn.

Expansion is plain text substitution: the partial's content replaces the directive in place, then the combined Markdown parses as one document. Two consequences follow from that, plus two rules that keep the feature predictable:

- **A block partial may hold many blocks** — paragraphs, lists, callouts, fences. Because the directive sits on its own line with blank lines around it, each block splices in as its own block. A trailing newline at the end of the partial is harmless here.
- **An inline partial must be a single run of text with no trailing newline.** A trailing newline splices a line break into the middle of the host sentence, ending the paragraph early; `_includes/sdk-version.md` is a single line (`10.0.100`) with no terminating newline for exactly this reason.
- A directive inside a fenced code block is left verbatim, so this page can show the syntax without expanding it.
- Relative links and images inside a partial are **not** rebased — they resolve as if written in the host page. Keep partials free of relative links, or use site-absolute paths.

## When a partial is missing

A target that does not exist collapses to an HTML comment instead of failing the build:

```html
<!-- Pennington: include not found: ../../../_includes/typo.md -->
```

View source on a page during `dotnet run` to spot a mistyped path — the comment names the directive that could not be resolved. Only local relative paths are spliced: an `[!INCLUDE …](https://…)` directive is also skipped, emitting `<!-- Pennington: include skipped (not a local file): … -->`. An include cycle is broken the same way, emitting `<!-- Pennington: include cycle broken: … -->`.

A missing include is the one failure mode you have to catch yourself. Neither the build report nor `dotnet run -- diag warnings` flags an unresolved directive — expansion happens inside the Markdown parser, which swallows the failure into the HTML comment and moves on. A typo'd path drops its content from the published site silently, so inspect the rendered output rather than trusting a clean build.

## Verify

- Run `dotnet run`, open a host page, and confirm the partial's text appears where the directive sat — block partials as their own blocks, an inline partial spliced into the sentence with no stray line break.
- Edit the partial (for example, change `_includes/sdk-version.md`) and reload every host page — each one reflects the new text on the next build.
- Mistype a path on purpose and view source: the body carries `<!-- Pennington: include not found: ... -->` and that block is empty. Search the rendered HTML for `Pennington: include` to sweep a page for skipped, missing, or cyclic directives — the build itself reports nothing.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — include directives alongside every other non-CommonMark feature
- How-to: <xref:how-to.rich-content.content-tabs> — tab a partial's worth of content per platform or language
- How-to: <xref:how-to.pages.front-matter> — the front-matter keys a host page still needs (a partial's own front matter is stripped on include)

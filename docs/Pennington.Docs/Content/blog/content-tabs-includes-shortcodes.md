---
title: Content tabs, includes, and shortcodes
description: Three Markdown authoring features: tabbed content panels, [!INCLUDE] partials, and shortcodes that stamp live values into the page.
author: Phil Scott
date: 2026-05-19
isDraft: false
tags:
  - markdown
  - authoring
---

Three Markdown features for writing docs: tabbed content, file includes, and
shortcodes. None of them need configuration. They're on by default.

## Content tabs

Pennington already tabbed adjacent code blocks. Content tabs go wider: a run of
level-1 headings that each link to `#tab/<id>`, closed by a `---`, becomes one
tabset, and each panel holds whatever Markdown you want (prose, a list, a
callout, a code block):

```markdown
# [macOS](#tab/macos)

Install the SDK with the `.pkg` installer or Homebrew.

# [Linux](#tab/linux)

Use your distribution's package manager.

# [Windows](#tab/windows)

Run `winget install Microsoft.DotNet.SDK.11`.

---
```

Tabs with the same id stay in sync across the page, and the choice carries
between pages: pick `macos` once and every OS tabset follows. The [content tabs
how-to](xref:how-to.rich-content.content-tabs) covers dependent tabs and the
markup.

## File includes

Shared boilerplate, like a prerequisite note or a support matrix, usually means
copy-paste that drifts. `[!INCLUDE]` pulls a Markdown file in before parsing, so
the host page renders as if you'd typed it inline:

```markdown
[!INCLUDE [prerequisites](../_includes/dotnet-note.md)]
```

It works as a block or mid-sentence, resolves nested includes (with cycle
detection), and skips fenced code so you can document the syntax itself. One
honest caveat: a missing include fails to an HTML comment, not a build error, so
check the rendered page. The [include
how-to](xref:how-to.pages.include-shared-content) has the rest.

## Shortcodes

A version number typed into an install command is wrong by the next release. A
shortcode runs at render time and splices its result in:

```bash
dotnet add package Pennington --version \<?# PackageVersion /?>
```

`PackageVersion` stamps Pennington's published NuGet version; `Version` reads
your own assembly's. Shortcodes expand inside code fences on purpose, so a
copyable command carries the real number. Write your own by implementing
`IShortcode` and registering it after `AddPennington`. The [shortcode
how-to](xref:how-to.markdown-pipeline.shortcodes) walks through one.

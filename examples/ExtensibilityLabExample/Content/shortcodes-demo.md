---
title: Shortcodes demo
description: Pre-render shortcodes expanded before Markdig parses the page.
---

# Shortcodes demo

Shortcodes are textual directives the renderer expands **before** Markdig
parses the page, so handler output flows through the rest of the pipeline
as regular markdown. The framework ships one (`Version`); this lab adds
`GitHubRepo` to show how a custom handler slots into the same dispatch
table.

## Built-in: Version

Write `\<?# Version /?>` and the renderer substitutes the entry
assembly's version: <?# Version /?>.

Pass `format=major` for just the leading component: <?# Version format=major /?>.

## Custom: GitHubRepo

`GitHubRepo` takes one positional argument — the repository slug — and
emits an anchor. Source: <?# GitHubRepo "anthropic/anthropic-sdk-python" /?>.

## Inside a fenced block, shortcodes still expand

The expander runs across the whole markdown source, so install snippets
can stamp the real version into a copyable command:

```bash
dotnet add package Pennington --version <?# Version /?>
```

To show a literal directive without expanding it — say, when documenting
the syntax in a code sample — prefix the opener with a backslash. The
expander consumes the backslash and emits the directive as-is:

```markdown
Run \\<?# Version /?> to stamp the host version.
```

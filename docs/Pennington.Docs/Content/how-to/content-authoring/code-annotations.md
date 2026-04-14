---
title: "Annotate code blocks"
description: "Apply line-highlight, diff add/remove, focus, and error/warning markers to fenced code blocks using trailing-comment `[!code …]` directives."
section: "content-authoring"
order: 60
tags: []
uid: how-to.content-authoring.code-annotations
isDraft: true
search: false
llms: false
---

> **In this page.** The `[!code highlight]`, `[!code ++]` / `[!code --]`, `[!code focus]`, and `[!code error]` / `[!code warning]` trailing-comment directives on fenced blocks.
>
> **Not in this page.** Writing a custom code-block preprocessor — see the extensibility how-to.

## When to use this

To draw attention to specific lines in a fenced code block, append a trailing comment `// [!code <notation>]` to the lines you want marked. Pennington uses per-line directives rather than brace-range forms like `{1,3}` or `{focus 1-3}`.

## Assumptions

- You have an existing Pennington site with markdown content (see the [Getting Started tutorial](/tutorials/getting-started/first-site) if not).
- Syntax highlighting is enabled (default when `AddPennington` is registered).

To copy a working setup, see [`examples/BeaconDocsExample/Content/guides/migration-v3.md`](https://github.com/usepennington/pennington/tree/main/examples/BeaconDocsExample/Content/guides/migration-v3.md) — it already uses `[!code --]`, `[!code ++]`, `[!code highlight]`, `[!code error]`, and `[!code warning]`.

---

## Steps

### 1. Highlight specific lines with `[!code highlight]`

Append a trailing comment using the host language's comment marker (`//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, `/*`). The notation is `[!code highlight]` (alias `[!code hl]`). Each tagged line gets the class `highlight`; the `<pre>` receives `has-highlighted`. Repeat the directive on every line you want highlighted — ranges like `{1,3}` are not parsed.

```markdown
```csharp
options.DefaultInterval = TimeSpan.FromMinutes(5);
options.AlertThreshold = 3; // [!code highlight]
options.TimeoutMs = 5000;   // [!code highlight]
options.EnableMetrics = true;
```
```

### 2. Mark diff add/remove with `[!code ++]` and `[!code --]`

Use `[!code ++]` for added lines and `[!code --]` for removed lines. Added lines receive class `diff-add`; removed lines receive `diff-remove`. The `<pre>` gets `has-diff` whenever either notation appears — style the gutter or background from there. Place added and removed lines in the same fence to render a unified diff view.

### 3. Focus a range with `[!code focus]`

Tag each line in the focused region with `// [!code focus]`. Focused lines get class `focused`; every other line is marked `blurred`; the `<pre>` gets `has-focused`. The default style dims blurred lines and restores them on `pre:hover`. There is no single-range shortcut — annotate each line of the range.

### 4. Flag problem lines with `[!code error]` and `[!code warning]`

Append `// [!code error]` or `// [!code warning]` to a line. Error lines get class `error`; warning lines get class `warning`. The `<pre>` receives `has-errors` and/or `has-warnings` so containers can display icons or legends. Useful for illustrating deprecated APIs or migration pitfalls alongside a working replacement.

### 5. Trailing-comment cleanup is automatic

The directive is stripped from the rendered line. If text follows the directive, the comment marker is preserved so the remaining comment reads naturally. Empty comment remnants (`//`, `#`, `<!-- -->`, `/* */`) are cleaned up automatically — no hand-trimming needed.

---

## Verify

- Run `dotnet run --project docs/Pennington.Docs` and open the page containing the annotated fence.
- Inspect the rendered HTML: the `<pre>` carries the expected aggregate class (`has-highlighted`, `has-diff`, `has-focused`, `has-errors`, `has-warnings`).
- Each annotated `<span class="line">` carries the matching per-line class (`highlight`, `diff-add`, `diff-remove`, `focused`, `blurred`, `error`, `warning`).
- The `[!code …]` directive text is absent from the visible rendered output.

## Related

- Reference: [Code-block argument reference](/reference/markdown/code-block-args)
- How-to: [Add a custom code-block preprocessor](/how-to/extensibility/code-block-preprocessor)
- Background: [Syntax highlighting](/explanation/rendering/highlighting)

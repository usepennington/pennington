---
title: "Annotate code blocks"
description: "Apply line-highlight, diff add/remove, focus, and error/warning markers to fenced code blocks."
section: "content-authoring"
order: 40
tags: []
uid: how-to.content-authoring.code-annotations
isDraft: true
search: false
llms: false
---

> **In this page.** Line-highlight ranges (`{1,3}`), diff add/remove (`{+1}`/`{-1}`), focus (`{focus 1-3}`), and error/warning markers on fenced blocks.
>
> **Not in this page.** Writing a custom `ICodeBlockPreprocessor` — see the extensibility how-to.

## When to use this

- Reader has fenced code in markdown and wants to draw attention to specific lines.
- Reader wants a migration-style before/after diff inside a single fence.
- Reader wants to mark a line as error/warning/focus to match the real `CodeTransformer` output.

## Assumptions

- Existing Pennington site with markdown content (see Getting Started tutorial if not).
- Syntax highlighting is enabled (default via `MarkdownPipelineFactory.CreateWithExtensions`).
- Reader accepts the real Pennington annotation grammar — per-line trailing comments `// [!code <notation>]`, not the `{1,3}` brace form used by some other generators.

To copy a working setup, see [`examples/BeaconDocsExample/Content/guides/migration-v3.md`](https://github.com/pkrumins/Pennington/tree/main/examples/BeaconDocsExample/Content/guides/migration-v3.md) for a page that already exercises `[!code --]`, `[!code ++]`, `[!code highlight]`, `[!code error]`, and `[!code warning]`.

---

## Steps

### 1. Highlight specific lines with `[!code highlight]`

- Append a trailing line comment using the host language's comment marker (`//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, `/*`).
- Notation is `[!code highlight]` (alias `[!code hl]`).
- Each tagged line gets class `highlight`; the `<pre>` receives `has-highlighted`.
- Repeat the directive on every line you want highlighted (ranges like `{1,3}` are not parsed — one directive per line).

```markdown
```csharp
options.DefaultInterval = TimeSpan.FromMinutes(5);
options.AlertThreshold = 3; // [!code highlight]
options.TimeoutMs = 5000;   // [!code highlight]
options.EnableMetrics = true;
```
```

### 2. Mark diff add/remove with `[!code ++]` and `[!code --]`

- Use `[!code ++]` for added lines and `[!code --]` for removed lines.
- Added lines receive class `diff-add`; removed lines receive class `diff-remove`.
- The `<pre>` gets `has-diff` whenever either notation appears — style the gutter/background from there.
- Place added and removed lines in the same fence to render a unified diff view.

### 3. Focus a range with `[!code focus]`

- Tag each line in the focused region with `// [!code focus]`.
- Focused lines get class `focused`; every other line is marked `blurred`; the `<pre>` gets `has-focused`.
- The default style dims blurred lines and restores them on `pre:hover` (see `MonorailCssOptions` selectors).
- There is no single-range shortcut (`{focus 1-3}`); annotate each line of the range.

### 4. Flag problem lines with `[!code error]` and `[!code warning]`

- Append `// [!code error]` or `// [!code warning]` to a line.
- Error lines get class `error`; warning lines get class `warning`.
- `<pre>` receives `has-errors` and/or `has-warnings` so containers can display icons or legends.
- Useful for illustrating deprecated APIs or migration pitfalls alongside a working replacement.

### 5. (Optional) Trim or replace the trailing comment

- `CodeTransformer.RemoveDirectiveFromLine` strips the whole directive when nothing else follows it.
- If text follows the directive on the same line, the comment marker is preserved so the remaining comment still reads naturally.
- Empty comment remnants (`//`, `#`, `<!-- -->`, `/* */`) are cleaned up automatically — you don't need to hand-trim.

---

## Verify

- Run `dotnet run --project docs/Pennington.Docs` and open the page containing the annotated fence.
- Inspect the rendered HTML: the `<pre>` element carries the expected aggregate class (`has-highlighted`, `has-diff`, `has-focused`, `has-errors`, `has-warnings`).
- Each annotated `<span class="line">` carries the matching per-line class (`highlight`, `diff-add`, `diff-remove`, `focused`, `blurred`, `error`, `warning`).
- The `[!code …]` directive text is absent from the visible rendered output.

## Related

- Reference: [Code-block argument reference](/reference/markdown/code-block-args)
- How-to: [Add a custom code-block preprocessor](/how-to/extensibility/code-block-preprocessor)
- Background: `src/Pennington/Markdown/Extensions/CodeTransformer.cs` (authoritative source for notation names and emitted classes)

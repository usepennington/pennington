---
title: "Annotating Code Blocks with Line Directives"
description: "Use diff, highlight, focus, error, warning, and word highlighting directives to draw attention to specific lines in fenced code blocks"
uid: "penn.how-to.annotating-code-blocks"
order: 12
---

## Beat 1: Diff Directives

You have a code block and want to draw attention to specific lines -- showing what changed, highlighting key lines, or marking errors.

Use `[!code ++]` and `[!code --]` line directives to produce green/red diff annotations that show additions and removals.

### What to show
- A code block where the old `var client = new BeaconClient();` line has `// [!code --]` appended, and the new `var monitor = new HttpMonitor();` line has `// [!code ++]` appended
- Reference `M:Penn.Markdown.Extensions.CodeTransformer.Transform(System.String)` which parses `[!code]` directives from comment markers
- Show the CSS class mapping: `++` maps to `diff-add`, `--` maps to `diff-remove` (defined in `CodeTransformer.GetCssClassForNotation`)
- The `<pre>` element receives a `has-diff` class when any diff directives are present

### Key points
- Directives are recognized inside any comment marker syntax (`//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, `/*`) as defined in `CodeTransformer.CommentMarkers`
- The directive comment is completely removed from the rendered output; only the CSS class on the `<span class="line">` element remains
- Show raw markdown and rendered result side by side

## Beat 2: Highlight and Focus Directives

Use `[!code highlight]` on changed lines and `[!code focus]` to dim everything else. These compose independently to create a visual spotlight effect.

### What to show
- A 12-line `appsettings.json` block where lines 4-5 have `// [!code highlight]` and the same lines have `// [!code focus]`
- `highlight` / `hl` maps to CSS class `highlight`; `focus` maps to `focused`
- When any line has `[!code focus]`, `CodeTransformer` adds `has-focused` to the `<pre>` element and `blurred` to every unfocused line
- When `[!code highlight]` is present, `has-highlighted` is added to `<pre>`

### Key points
- Focus and highlight compose independently: a line can be both focused AND highlighted
- The blurring is applied to all lines that are NOT focused, creating a visual spotlight effect
- Reference the `ApplyTransformationsToDom` logic in `T:Penn.Markdown.Extensions.CodeTransformer`

## Beat 3: Word Highlighting

Use `[!code word:BeaconClient]` to highlight every occurrence of a word in a line. Add a tooltip with `[!code word:HttpMonitor|renamed from BeaconClient]`.

### What to show
- A code block with `// [!code word:BeaconClient]` on one line, causing every occurrence of "BeaconClient" in that line to be wrapped in `<span class="word-highlight">`
- The tooltip variant: `// [!code word:HttpMonitor|renamed from BeaconClient]` wraps the word in `<span class="word-highlight-with-message">` and appends a `<div class="word-highlight-message">` callout
- Reference `CodeTransformer.ParseWordHighlight` which splits on `|` to extract word and optional message
- Reference `CodeTransformer.ApplyWordHighlighting` which finds text nodes containing the word and wraps them
- The `<pre>` receives `has-word-highlights` when any word directive is present

### Key points
- Word highlighting operates on the first occurrence of the word in the line's text nodes
- The message callout includes an arrow container (`word-highlight-arrow-outer`, `word-highlight-arrow-inner`) for tooltip-style positioning
- The directive comment is removed; the word highlight is purely visual markup

## Beat 4: Error and Warning Annotations

Mark a deprecated method call with `[!code error]` and a performance-sensitive line with `[!code warning]`. These annotate existing code rather than showing additions or removals.

### What to show
- A code block where `client.SendLegacy()` has `// [!code error]` and `monitor.SendBatched()` has `// [!code warning]`
- `error` maps to CSS class `error` on the line `<span>`; `warning` maps to `warning`
- The `<pre>` element receives `has-errors` and/or `has-warnings` classes respectively

### Key points
- Error and warning are distinct from diff: they annotate existing code rather than showing additions/removals
- These are purely visual annotations; they do not affect build diagnostics in `T:Penn.Generation.BuildReport`

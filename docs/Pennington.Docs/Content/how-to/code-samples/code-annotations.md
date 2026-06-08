---
title: "Annotate specific lines in a code block"
description: "Apply highlight, diff, focus, and error/warning line classes to fenced code with trailing `[!code ...]` comment directives."
uid: how-to.code-samples.code-annotations
order: 1
sectionLabel: "Code Samples"
tags: [authoring, code, highlighting, annotations]
---

To call out specific lines in a fenced code block — emphasizing a change, diffing before/after, focusing attention, or surfacing a diagnostic — append a trailing `[!code ...]` comment directive to the line. The `CodeTransformer` runs after the highlighter, promotes the directive's class onto the enclosing `.line` span, and deletes the comment so the rendered code stays clean. For why the transformer sits where it does in the pipeline, see <xref:explanation.rendering.highlighting>.

## Before you begin
- An existing Pennington site renders markdown with highlighted code fences (see <xref:tutorials.getting-started.first-site> if not).
- The fenced language supports a comment syntax the transformer recognizes — `//`, `#`, `--`, `<!-- -->`, `*`, `%`, `'`, `REM`, `;`, or `/* */`.

## Annotation directives

Each H3 below shows the source markdown above the rendered fence. Swap the comment marker to match the fenced language (`//` for C#/JS, `#` for YAML/Python, `--` for SQL, `<!-- -->` for HTML).

### Highlight a single line

Append `// [!code highlight]` to the line. The transformer adds the `highlight` class to the matching `.line` span.

````markdown
```csharp
public int Add(int a, int b)
{
    return a + b; // [!code highlight]
}
```
````

```csharp
public int Add(int a, int b)
{
    return a + b; // [!code highlight]
}
```

### Mark added or removed lines

Use `[!code ++]` for added lines and `[!code --]` for removed lines. The transformer swaps each directive for `diff-add` or `diff-remove` on the `.line` span so the stylesheet can paint the gutter.

````markdown
```csharp
public int Multiply(int a, int b) // [!code ++]
{
    return a * b; // [!code ++]
}
public int OldWay(int a, int b) // [!code --]
{
    return a + b; // [!code --]
}
```
````

```csharp
public int Multiply(int a, int b) // [!code ++]
{
    return a * b; // [!code ++]
}
public int OldWay(int a, int b) // [!code --]
{
    return a + b; // [!code --]
}
```

### Focus one line and blur the rest

Add `[!code focus]` to the line (or lines) worth zeroing in on. The focused lines receive a `focused` class, every other line receives a `blurred` class, and the enclosing `<pre>` receives `has-focused` so the stylesheet can scope the effect.

````markdown
```csharp
var config = new Config(); // [!code focus]
config.Apply();
config.Save();
```
````

```csharp
var config = new Config(); // [!code focus]
config.Apply();
config.Save();
```

### Flag errors and warnings

Use `[!code error]` and `[!code warning]` to show diagnostics inline. The transformer applies `error` and `warning` classes so the rendered lines read like compiler output.

````markdown
```csharp
var path = null; // [!code error]
var length = path.Length; // [!code warning]
```
````

```csharp
var path = null; // [!code error]
var length = path.Length; // [!code warning]
```

## Verify

- Run `dotnet run` and load the page with the annotated fence. The `[!code …]` comments are gone from the rendered HTML.
- View source: each annotated `.line` span carries the matching class (`highlight`, `diff-add`, `diff-remove`, `focused` / `blurred`, `error`, `warning`); a fence with any focused line also carries `has-focused` on its `<pre>`.

## What the renderer emits

The `CodeTransformer` promotes each directive's class onto the enclosing `.line` span — one of `highlight`, `diff-add`, `diff-remove`, `focused`, `blurred`, `error`, `warning` — and deletes the trailing comment so the directive text never appears in rendered HTML. Fences with at least one `[!code focus]` also receive `has-focused` on the `<pre>` so the stylesheet can blur the non-focused lines. Comment-marker variants (`#`, `--`, `<!-- -->`, and so on) are recognized the same way, so the same directive set works across languages.

See <xref:explanation.rendering.highlighting> for why the transformer runs after the highlighter.

## Related

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter) — `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Register a code-block preprocessor](xref:how-to.markdown-pipeline.code-block-preprocessor) — when a trailing-comment directive isn't enough and fence bodies need transformation
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why the transformer runs after the highlighter and where custom highlighters plug in

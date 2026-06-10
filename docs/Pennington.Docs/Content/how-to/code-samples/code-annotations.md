---
title: "Annotate specific lines in a code block"
description: "Apply highlight, diff, focus, and error/warning line classes to fenced code with trailing `[!code ...]` comment directives."
uid: how-to.code-samples.code-annotations
order: 1
sectionLabel: "Code Samples"
tags: [authoring, code, highlighting, annotations]
---

To call out specific lines in a fenced code block — emphasizing a change, diffing before/after, focusing attention, or surfacing a diagnostic — append a trailing `[!code ...]` comment directive to the line. Pennington promotes the directive onto the enclosing line and strips the comment, so the rendered code stays clean and the called-out line picks up a CSS class you can style.

## Before you begin
- An existing Pennington site renders markdown with highlighted code fences (see <xref:tutorials.getting-started.first-site> if not).
- The fenced language supports one of the recognized comment markers — `//`, `#`, `--`, `<!-- -->`, `*`, `%`, `'`, `REM`, `;`, or `/* */`.

## Annotation directives

Each H3 below shows the source markdown above the rendered fence. Swap the comment marker to match the fenced language (`//` for C#/JS, `#` for YAML/Python, `--` for SQL, `<!-- -->` for HTML).

### Highlight a single line

Append `// [!code highlight]` to the line.

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

Use `[!code ++]` for added lines and `[!code --]` for removed lines.

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

Add `[!code focus]` to the line (or lines) worth zeroing in on. Every other line in the fence blurs back so the focused line stands out.

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

Use `[!code error]` and `[!code warning]` to show diagnostics inline. The rendered lines read like compiler output.

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

## Style the annotation classes

Each directive promotes a CSS class onto the enclosing `.line` span — `highlight`, `diff-add`, `diff-remove`, `focused`, `blurred`, `error`, or `warning` — and a fence with any focused line also carries `has-focused` on its `<pre>`. On a site that uses `Pennington.MonorailCss`, these classes are styled out of the box: the highlight tint, diff gutter signs, blur effect, and diagnostic backgrounds all render with no extra work. A host that ships its own CSS instead must define each class itself — without those rules the directives still resolve and strip the comments, but the lines render unstyled.

## Verify

- Run `dotnet run` and load the page with the annotated fence. The highlighted line renders with a tinted background, diff lines show `+`/`-` gutters, the focused line stays sharp while the rest blur, and the `[!code …]` comments are gone.
- View source: each annotated `.line` span carries the matching class, and a fence with any focused line carries `has-focused` on its `<pre>`.

## Related

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter) — `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Register a code-block preprocessor](xref:how-to.markdown-pipeline.code-block-preprocessor) — when a trailing-comment directive isn't enough and fence bodies need transformation
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why directive promotion runs after the highlighter and where custom highlighters plug in

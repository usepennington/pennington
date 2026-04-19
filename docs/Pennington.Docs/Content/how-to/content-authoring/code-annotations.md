---
title: "Annotate code blocks"
description: "Apply highlight, diff, focus, and error/warning line classes to fenced code with trailing `[!code ...]` comment directives."
uid: how-to.content-authoring.code-annotations
order: 201060
sectionLabel: Content Authoring
tags: [authoring, code, highlighting, annotations]
---

To call out specific lines in a fenced code block — highlighting a change, diffing before/after, focusing attention, or surfacing a diagnostic — trailing `[!code ...]` comment directives handle it without a custom extension. For why the transformer runs after the highlighter and how highlighters plug in, see <xref:explanation.rendering.highlighting>.

## Assumptions

- An existing Pennington site renders markdown with highlighted code fences (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- The fenced language supports a comment syntax the transformer recognises — `//`, `#`, `--`, `<!-- -->`, `*`, `%`, `'`, `REM`, `;`, or `/* */`.
- Authoring happens in plain markdown, not injected HTML — the directives are parsed from the rendered highlighter output and travel through the fence as comments.

For a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/exampleKitchenSinkExample) — `Content/main/code-annotations.md` stages one fence per directive and is the fixture this page embeds.

---

## Steps

<Steps>
<Step StepNumber="1">

**Highlight a single line**

Append `// [!code highlight]` to the line for emphasis, swapping the comment marker to match the fenced language (`#` for YAML, `--` for SQL). The transformer strips the directive and adds the `highlight` class to the `.line` span.

````markdown
```csharp
public int Add(int a, int b)
{
    return a + b; // [!code highlight]
}
```
````

</Step>
<Step StepNumber="2">

**Mark lines as added or removed (diff)**

Use `[!code ++]` for added lines and `[!code --]` for removed lines. Each directive is replaced with a `diff-add` or `diff-remove` class on the `.line` span so the stylesheet can paint the gutter.

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

</Step>
<Step StepNumber="3">

**Focus one line and dim the rest**

Add `[!code focus]` to the line (or lines) worth zeroing in on. Every other line receives a `dimmed` class so the focused line stands out.

````markdown
```csharp
var config = new Config(); // [!code focus]
config.Apply();
config.Save();
```
````

</Step>
<Step StepNumber="4">

**Flag errors and warnings**

Use `[!code error]` and `[!code warning]` to surface diagnostics inline. The transformer applies `error` and `warning` classes to the respective lines so they render like compiler output.

````markdown
```csharp
var path = null; // [!code error]
var length = path.Length; // [!code warning]
```
````

</Step>
<Step StepNumber="5">

**Confirm directives are stripped from the final HTML**

The `CodeTransformer` post-processes the highlighter output: it promotes each directive's class onto the enclosing `.line` span and deletes the trailing comment, so the rendered code stays clean. The fixture below shows all four directives end-to-end as they appear in source markdown.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/code-annotations.md
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run` and visit the page — each annotated line shows the expected treatment (highlighted bar, diff marker, dimmed siblings, error/warning paint) and the `// [!code ...]` text is absent from the rendered output.
- View source on the rendered `<pre>` — annotated `.line` spans carry the matching class (`highlight`, `diff-add`, `diff-remove`, `focused`, `dimmed`, `error`, `warning`).
- Swap the comment marker to match a different fenced language (for example `# [!code highlight]` in a `yaml` block) — the directive is stripped and the class is still applied.

## Related

- Reference: [Highlighting interfaces](xref:reference.api.i-code-highlighter) — `ICodeHighlighter`, `ICodeBlockPreprocessor`, `HighlightingService`, and `TextMateLanguageRegistry`
- How-to: [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor) — when a trailing-comment directive isn't enough and fence bodies need transformation
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting) — why the transformer runs after the highlighter and where custom highlighters plug in

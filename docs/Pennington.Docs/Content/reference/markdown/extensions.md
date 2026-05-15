---
title: "Markdown extensions catalog"
description: "Every non-CommonMark Markdown feature Pennington adds — tabs, alerts, code annotations, and cross-reference tags — with syntax, arguments, and emitted classes."
sectionLabel: "Markdown Extensions"
order: 403010
tags: [markdown, extensions, alerts, tabs]
uid: reference.markdown.extensions
---

The catalog of non-CommonMark Markdown features wired into Pennington's Markdig pipeline. Markdig's own built-in syntax (tables, footnotes, and so on) is not covered here.

| Extension | Syntax | Controlled by | Doc page |
|---|---|---|---|
| Tabs | Adjacent fences with `tabs=true` | `UseTabbedCodeBlocks` | [Tabbed code](xref:how-to.code-samples.tabbed-code) |
| Alerts | `> [!KIND]` inside blockquote | `UseCustomAlerts` | [Alerts](xref:how-to.rich-content.alerts) |
| Code annotations | Trailing-comment `[!code …]` directive | `UseSyntaxHighlighting` | [Code annotations](xref:how-to.code-samples.code-annotations) |
| Cross-reference tags | `<xref:uid>` or `href="xref:uid"` | `XrefHtmlRewriter` (response stage) | [Cross-references](xref:how-to.navigation.cross-references) |

## Tabs

Renders a run of consecutive fenced code blocks (starting with one that carries `tabs=true`) as a single tabbed container with `role="tablist"`, `role="tab"`, and panel regions. The first tab is active by default.

### Syntax

````markdown
```csharp tabs=true title="C#"
// block A
```

```razor title="Razor"
@* block B *@
```
````

Each fenced block in the consecutive run becomes a tab panel; only the first block requires `tabs=true` to open the group.

### Arguments

| Name | Type | Default | Description |
|---|---|---|---|
| `tabs` | `true` | (absent) | Applies to the first fence in the group. Marks a fenced block as the start of a tabbed run; consecutive subsequent fences join the same group. |
| `title` | `string` (optionally quoted) | pretty language name derived from the info string | Applies to each fence in the group. Overrides the label shown on the tab button. |

Arguments are `key=value` pairs; quoted values are allowed. See <xref:reference.markdown.code-block-args> for the full grammar.

### Emitted CSS classes

| Option | Default class | Role |
|---|---|---|
| `OuterWrapperCss` | `not-prose` | Outer `<div>` wrapper that opts out of prose styling. |
| `ContainerCss` | `tab-container` | Container wrapping tablist and panels. |
| `TabListCss` | `tab-list` | `role="tablist"` row. |
| `TabButtonCss` | `tab-button` | `role="tab"` `<button>` (carries `data-state="active"|"inactive"`). |
| `TabPanelCss` | `tab-panel` | `aria-labelledby`-bound panel wrapping the rendered code block. |

Classes are configurable via `TabbedCodeBlockRenderOptions` passed to `UseTabbedCodeBlocks`.

### Minimal example

Markdown source showing a two-fence tabbed group:

```markdown:path
examples/DocSitePagesAndLinksExample/snippets/markdown-tabs-example.md
```

## Alerts

Parses a GitHub-flavored `> [!KIND]` token as the first line of a blockquote and emits an `AlertBlock` with two CSS classes. The blockquote form is the only accepted syntax.

### Syntax

````markdown
> [!NOTE]
> Body text of the alert, CommonMark rendered.
````

`KIND` is a case-insensitive alphabetic token; unrecognized kinds still parse and emit a `markdown-alert-<kind>` class using the lowercased token.

### Arguments

| Name | Type | Default | Description |
|---|---|---|---|
| `KIND` | identifier | — | One of `NOTE`, `TIP`, `CAUTION`, `WARNING`, `IMPORTANT`. Case-insensitive. Unrecognized tokens still parse, emitting `markdown-alert-<kind>` using the lowercased value. |

### Built-in kinds and emitted CSS classes

Every alert receives two classes: `markdown-alert` (constant) and `markdown-alert-<kind>`.

| Kind | Secondary class | Typical use |
|---|---|---|
| `NOTE` | `markdown-alert-note` | Supplementary information. |
| `TIP` | `markdown-alert-tip` | Helpful aside. |
| `CAUTION` | `markdown-alert-caution` | Risky operation. |
| `WARNING` | `markdown-alert-warning` | Something likely to go wrong. |
| `IMPORTANT` | `markdown-alert-important` | Must-read information. |

### Minimal example

Markdown excerpt showing a `[!NOTE]` block in context:

```markdown:path
examples/DocSitePagesAndLinksExample/snippets/markdown-alert-example.md
```

## Code annotations

After syntax highlighting, each rendered line is scanned for a `[!code …]` directive inside a language-appropriate comment. The directive is stripped and a CSS class is applied to the line (and optionally to the enclosing `<pre>`). The `word:` variant wraps a matching substring; the `include-start`/`include-end`/`exclude-start`/`exclude-end` directives remove surrounding lines from the output.

### Syntax

````markdown
```csharp
var x = 1; // [!code highlight]
var y = 2; // [!code ++]
var z = 3; // [!code word:z|renamed from q]
```
````

The directive must appear inside a recognized comment marker for the language (`//`, `#`, `--`, `<!-- -->`, `*`, `%`, `'`, `REM`, `;`, `/* */`). The directive and any now-empty comment wrapper are removed, leaving trailing content intact.

### Notations

| Directive | Line class | `<pre>` class | Description |
|---|---|---|---|
| `highlight` (alias `hl`) | `highlight` | `has-highlighted` | Marks a line as emphasized. |
| `++` | `diff-add` | `has-diff` | Marks a line as an addition. |
| `--` | `diff-remove` | `has-diff` | Marks a line as a removal. |
| `focus` | `focused` | `has-focused` | Focuses listed lines; all others receive `blurred`. |
| `error` | `error` | `has-errors` | Marks a line as an error. |
| `warning` | `warning` | `has-warnings` | Marks a line as a warning. |
| `word:TEXT` | — | `has-word-highlights` | Wraps the first occurrence of `TEXT` in `<span class="word-highlight">`. Example: `[!code word:Multiply]`. |
| `word:TEXT\|MESSAGE` | — | `has-word-highlights` | Wraps the match in `word-highlight-with-message` and renders an adjacent `word-highlight-message` callout containing `MESSAGE`. Example: `[!code word:queue\|renamed from buffer]`. |
| `include-start` / `include-end` | — | — | Structural. Keep only lines between matching markers; markers are removed. |
| `exclude-start` / `exclude-end` | — | — | Structural. Drop lines between matching markers; markers are removed. |

### Emitted CSS classes

Line-level classes (`highlight`, `diff-add`, `diff-remove`, `focused` / `blurred`, `error`, `warning`) are added to the `<span class="line">` wrapper. Block-level classes (`has-highlighted`, `has-diff`, `has-focused`, `has-errors`, `has-warnings`, `has-word-highlights`) are added to the outer `<pre>`. The `word:` notation emits `word-highlight` or `word-highlight-with-message` on the wrapped span, plus the callout elements `word-highlight-wrapper`, `word-highlight-message`, `word-highlight-arrow-container`, `word-highlight-arrow-outer`, and `word-highlight-arrow-inner`.

### Minimal example

An annotated fence exercising `[!code highlight]`, `[!code ++]`, and `[!code --]`; the enclosing `<pre>` receives `has-highlighted` and `has-diff`, and the trailing directive comments are stripped from the emitted HTML:

````markdown
```csharp
var message = "hello";   // [!code highlight]
var added = "added";     // [!code ++]
var removed = "gone";    // [!code --]
```
````

## Cross-reference tags

`xref:` links resolve after rendering against the uid-to-route map built from every page's front-matter `uid:`. Two surface forms are supported: the tag form `<xref:uid>` and the attribute form `[text](xref:uid)`. Unknown uids emit a diagnostic that surfaces in the dev overlay and in the static-build report.

### Syntax

````markdown
See <xref:reference.api.pennington-options>.

See [PenningtonOptions](xref:reference.api.pennington-options).
````

`uid` is the exact string declared in a page's front-matter `uid:` key.

### Arguments

| Name | Type | Default | Description |
|---|---|---|---|
| `uid` | identifier | — | Exact string declared in a page's front-matter `uid:` key. The tag form derives link text from the target page's title; the attribute form uses the supplied link text verbatim. |

### Emitted CSS classes

The rewriter emits a standard `<a href="…">` element with no added class; styling is delegated to the surrounding prose stylesheet.

### Minimal example

Both surface forms resolving the same uid — the tag form derives its link text from the target page title, the attribute form uses the supplied label verbatim:

```markdown
See <xref:reference.api.pennington-options> for the full options catalog.

Configure MonorailCSS through [the options record](xref:reference.api.monorail-css-options).
```

## See also

- How-to: [Tabbed code](xref:how-to.code-samples.tabbed-code)
- How-to: [Alerts](xref:how-to.rich-content.alerts)
- How-to: [Code annotations](xref:how-to.code-samples.code-annotations)
- How-to: [Cross-references](xref:how-to.navigation.cross-references)
- Related reference: [Code-block argument reference](xref:reference.markdown.code-block-args)

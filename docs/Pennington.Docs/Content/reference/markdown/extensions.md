---
title: "Markdown extensions catalog"
description: "Every non-CommonMark Markdown feature Pennington adds — tabs, alerts, code annotations, and cross-reference tags — with syntax, arguments, and emitted classes."
sectionLabel: "Markdown Extensions"
order: 10
tags: [markdown, extensions, alerts, tabs]
uid: reference.markdown.extensions
---

> **In this page.** _One sentence — paste/trim from TOC "Covers": every non-CommonMark feature in one scannable page — tabs, alerts (including the five built-in kinds' emitted CSS classes), code annotations, cross-reference tags — with syntax, arguments, and a minimal example each._
>
> **Not in this page.** _One sentence — paste/trim from TOC "Does not cover": Markdig core syntax (tables, footnotes, etc.) — those follow Markdig's own docs._

## Summary

_**One sentence: what it is.** E.g., "The catalog of non-CommonMark Markdown features wired into Pennington's Markdig pipeline."_
_**One sentence: where it lives.** E.g., "Extensions are registered in `MarkdownPipelineFactory.CreateWithExtensions` from `Pennington.Markdown.Extensions` (alerts, tabs, highlighting/code annotations) and `Pennington.Infrastructure` (xref resolution)."_

## Overview

_Single scannable table listing every extension covered below. Keep rows in the same order as the subsections. The "Controlled by" column names the pipeline hook that enables the feature; the "Doc page" column links to the matching how-to for task-oriented usage._

| Extension | Syntax | Controlled by | Doc page |
|---|---|---|---|
| Tabs | Adjacent fences with `tabs=true` | `UseTabbedCodeBlocks` | [Tabbed code](xref:how-to.content-authoring.tabbed-code) |
| Alerts | `> [!KIND]` inside blockquote | `UseCustomAlerts` | [Alerts](xref:how-to.content-authoring.alerts) |
| Code annotations | Trailing-comment `[!code …]` directive | `UseSyntaxHighlighting` | [Code annotations](xref:how-to.content-authoring.code-annotations) |
| Cross-reference tags | `<xref:uid>` or `href="xref:uid"` | `XrefHtmlRewriter` (response stage) | [Cross-references](xref:how-to.content-authoring.cross-references) |

## Tabs

_Two-sentence opener: what the extension does and the shape it consumes. Mention that the extension collapses a run of consecutive fenced code blocks (starting with one that carries `tabs=true`) into one tabbed container rendered as `role="tablist"` / `role="tab"` / panel regions; the first tab is active by default._

### Syntax

````markdown
```csharp tabs=true title="C#"
// block A
```

```razor title="Razor"
@* block B *@
```
````

_One sentence: each fenced block in the consecutive run becomes a tab panel; only the first block needs `tabs=true` to open the group._

### Arguments

| Name | Values | Default | Applies to | Description |
|---|---|---|---|---|
| `tabs` | `true` | — (absent) | First fence in the group | Marks a fenced block as the start of a tabbed run; consecutive subsequent fences join the same group. |
| `title` | string (optionally quoted) | Pretty language name derived from the info string | Each fence in the group | Overrides the label shown on the tab button. |

_Argument parsing is `key=value` pairs, quoted values allowed; see [Code-block argument reference](xref:reference.markdown.code-block-args) for the full grammar._

### Emitted CSS classes

| Option | Default class | Role |
|---|---|---|
| `OuterWrapperCss` | `not-prose` | Outer `<div>` wrapper that opts out of prose styling. |
| `ContainerCss` | `tab-container` | Container wrapping tablist and panels. |
| `TabListCss` | `tab-list` | `role="tablist"` row. |
| `TabButtonCss` | `tab-button` | `role="tab"` `<button>` (carries `data-state="active"|"inactive"`). |
| `TabPanelCss` | `tab-panel` | `aria-labelledby`-bound panel wrapping the rendered code block. |

_Classes are configurable via `TabbedCodeBlockRenderOptions` passed to `UseTabbedCodeBlocks`; the options record shape:_

```csharp:xmldocid
T:Pennington.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions
```

### Minimal example

_One sentence: markdown source excerpt from the DocSite authoring example, showing the two-fence tabbed group that the tutorial renders._

```csharp:xmldocid,bodyonly
M:DocSiteAuthorExample.Stage3.Source
```

## Alerts

_Two-sentence opener: what the extension does — parses GitHub-flavored `> [!KIND]` as the first line of a blockquote and replaces the `QuoteBlock` with an `AlertBlock` carrying two CSS classes. Note that Pennington registers its own `CustomAlertInlineParser` ahead of Markdig's built-in alert parser; the quote-block form is intentionally the only accepted syntax._

### Syntax

````markdown
> [!NOTE]
> Body text of the alert, CommonMark rendered.
````

_One sentence: `KIND` is a case-insensitive alphabetic run; unrecognized kinds still parse but emit a `markdown-alert-<kind>` class with the lowercased token._

### Arguments

_Alerts take no arguments — the kind token is the only variable, and it is drawn from the set listed under Emitted CSS classes._

### Built-in kinds and emitted CSS classes

_Every alert receives **two** classes: `markdown-alert` (constant) and `markdown-alert-<kind>` (derived from the lowercased token). The five built-in GitHub-compatible kinds and their emitted secondary classes are:_

| Kind token | Secondary class | Typical use |
|---|---|---|
| `NOTE` | `markdown-alert-note` | Supplementary information. |
| `TIP` | `markdown-alert-tip` | Helpful aside. |
| `CAUTION` | `markdown-alert-caution` | Risky operation — consequences before action. |
| `WARNING` | `markdown-alert-warning` | Something likely to go wrong. |
| `IMPORTANT` | `markdown-alert-important` | Must-read information. |

_Backing parser and class-emission logic:_

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CustomAlertInlineParser
```

### Minimal example

_One sentence: markdown excerpt from the DocSite authoring example showing a `[!NOTE]` block in context._

```csharp:xmldocid,bodyonly
M:DocSiteAuthorExample.Stage2.Source
```

## Code annotations

_Two-sentence opener: what the extension does — after syntax highlighting, `CodeTransformer` scans each rendered line for a `[!code …]` directive inside a language-appropriate comment, strips the comment, and applies a CSS class to the line (and sometimes to the enclosing `<pre>`). Includes the `word:` variant, which wraps a matching substring in a span rather than acting on the whole line, and the `include-start/include-end/exclude-start/exclude-end` snippet directives, which drop surrounding lines from the output._

### Syntax

````markdown
```csharp
var x = 1; // [!code highlight]
var y = 2; // [!code ++]
var z = 3; // [!code word:z|renamed from q]
```
````

_One sentence: the directive must sit inside a recognized comment marker for the language (`//`, `#`, `--`, `<!-- -->`, `*`, `%`, `'`, `REM`, `;`, `/* */`); the directive and a now-empty comment are removed, leaving trailing content intact._

### Arguments (notations)

| Notation | Line class | `<pre>` class added | Effect |
|---|---|---|---|
| `highlight` (alias `hl`) | `highlight` | `has-highlighted` | Marks a line as emphasized. |
| `++` | `diff-add` | `has-diff` | Marks a line as an addition. |
| `--` | `diff-remove` | `has-diff` | Marks a line as a removal. |
| `focus` | `focused` | `has-focused` | Focuses listed lines; all others receive `blurred`. |
| `error` | `error` | `has-errors` | Marks a line as an error. |
| `warning` | `warning` | `has-warnings` | Marks a line as a warning. |
| `word:<text>` | — (wraps substring) | `has-word-highlights` | Wraps first occurrence of `<text>` in `<span class="word-highlight">`. |
| `word:<text>\|<msg>` | — (wraps + callout) | `has-word-highlights` | As above, but wraps in `word-highlight-with-message` with an adjacent `word-highlight-message` callout. |
| `include-start` / `include-end` | — (structural) | — | Keep only lines between the matching start/end markers; markers are removed. |
| `exclude-start` / `exclude-end` | — (structural) | — | Drop lines between the matching start/end markers; markers are removed. |

_Backing transformer:_

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CodeTransformer
```

### Emitted CSS classes

_Summary of the classes listed above: line-level classes (`highlight`, `diff-add`, `diff-remove`, `focused`/`blurred`, `error`, `warning`) are added to the `<span class="line">` wrapper; `<pre>`-level classes (`has-highlighted`, `has-diff`, `has-focused`, `has-errors`, `has-warnings`, `has-word-highlights`) are added to the outer `<pre>` so stylesheets can style the block as a whole. `word:` notations emit `word-highlight`, `word-highlight-with-message`, and the callout pieces `word-highlight-wrapper` / `word-highlight-message` / `word-highlight-arrow-container` / `word-highlight-arrow-outer` / `word-highlight-arrow-inner`._

### Minimal example

_One sentence: an annotated fence exercising `[!code highlight]` on one line and `[!code ++]` / `[!code --]` on others — the `<pre>` inherits `has-highlighted` and `has-diff` and the trailing comments are stripped from the emitted output._

````markdown
```csharp
var message = "hello";   // [!code highlight]
var added = "added";     // [!code ++]
var removed = "gone";    // [!code --]
```
````

## Cross-reference tags

_Two-sentence opener: what the extension does — `xref:` links authored in markdown are resolved after render by `XrefHtmlRewriter` against the uid → route map owned by `XrefResolver`. Two surface forms are supported: a tag-shaped form `<xref:uid>` handled in a pre-parse string pass (because it is not valid HTML), and an attribute form `[text](xref:uid)` whose rendered `href="xref:uid"` is rewritten during the DOM pass; unknown uids emit a diagnostic via `DiagnosticContext` and surface in the dev overlay._

### Syntax

````markdown
See <xref:reference.options.pennington-options>.

See [PenningtonOptions](xref:reference.options.pennington-options).
````

_One sentence: `uid` is the exact string declared in a page's front-matter `uid:` key._

### Arguments

_The only variable is the `uid` token. Text content for the tag form is derived from the target page's title; the attribute form uses the supplied link text verbatim._

### Emitted CSS classes

_The rewriter emits a standard `<a href="…">` element with no added class — styling is delegated to the surrounding prose stylesheet._

### Minimal example

_One sentence: both surface forms resolving the same uid — the tag form derives its link text from the target page's title, the attribute form uses the supplied label verbatim._

```markdown
See <xref:reference.options.pennington-options> for the full options catalog.

Configure MonorailCSS through [the options record](xref:reference.options.monorail-css-options).
```

_Backing rewriter:_

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

## See also

- How-to: [Tabbed code](xref:how-to.content-authoring.tabbed-code)
- How-to: [Alerts](xref:how-to.content-authoring.alerts)
- How-to: [Code annotations](xref:how-to.content-authoring.code-annotations)
- How-to: [Cross-references](xref:how-to.content-authoring.cross-references)
- Related reference: [Code-block argument reference](xref:reference.markdown.code-block-args)

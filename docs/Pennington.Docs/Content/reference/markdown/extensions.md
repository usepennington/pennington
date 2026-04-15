---
title: "Markdown extensions catalog"
description: "Every non-CommonMark Markdown feature Pennington adds — tabs, alerts, code annotations, and cross-reference tags — with syntax, arguments, and emitted classes."
sectionLabel: "Markdown Extensions"
order: 403010
tags: [markdown, extensions, alerts, tabs]
uid: reference.markdown.extensions
---

The catalog of non-CommonMark Markdown features wired into Pennington's Markdig pipeline. Extensions are registered in `MarkdownPipelineFactory.CreateWithExtensions` from `Pennington.Markdown.Extensions` (alerts, tabs, highlighting/code annotations) and `Pennington.Infrastructure` (xref resolution). Markdig's own built-in syntax (tables, footnotes, and so on) is not covered here.

| Extension | Syntax | Controlled by | Doc page |
|---|---|---|---|
| Tabs | Adjacent fences with `tabs=true` | `UseTabbedCodeBlocks` | [Tabbed code](xref:how-to.content-authoring.tabbed-code) |
| Alerts | `> [!KIND]` inside blockquote | `UseCustomAlerts` | [Alerts](xref:how-to.content-authoring.alerts) |
| Code annotations | Trailing-comment `[!code …]` directive | `UseSyntaxHighlighting` | [Code annotations](xref:how-to.content-authoring.code-annotations) |
| Cross-reference tags | `<xref:uid>` or `href="xref:uid"` | `XrefHtmlRewriter` (response stage) | [Cross-references](xref:how-to.content-authoring.cross-references) |

## Tabs

The tabs extension collapses a run of consecutive fenced code blocks — starting with one that carries `tabs=true` — into a single tabbed container rendered as `role="tablist"` / `role="tab"` / panel regions, with the first tab active by default.

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

| Name | Values | Default | Applies to | Description |
|---|---|---|---|---|
| `tabs` | `true` | — (absent) | First fence in the group | Marks a fenced block as the start of a tabbed run; consecutive subsequent fences join the same group. |
| `title` | string (optionally quoted) | Pretty language name derived from the info string | Each fence in the group | Overrides the label shown on the tab button. |

Arguments are `key=value` pairs; quoted values are allowed. See [Code-block argument reference](xref:reference.markdown.code-block-args) for the full grammar.

### Emitted CSS classes

| Option | Default class | Role |
|---|---|---|
| `OuterWrapperCss` | `not-prose` | Outer `<div>` wrapper that opts out of prose styling. |
| `ContainerCss` | `tab-container` | Container wrapping tablist and panels. |
| `TabListCss` | `tab-list` | `role="tablist"` row. |
| `TabButtonCss` | `tab-button` | `role="tab"` `<button>` (carries `data-state="active"|"inactive"`). |
| `TabPanelCss` | `tab-panel` | `aria-labelledby`-bound panel wrapping the rendered code block. |

Classes are configurable via `TabbedCodeBlockRenderOptions` passed to `UseTabbedCodeBlocks`:

```csharp:xmldocid
T:Pennington.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions
```

### Minimal example

Markdown source showing a two-fence tabbed group from the DocSite authoring example:

```csharp:xmldocid,bodyonly
M:DocSiteAuthorExample.Stage3.Source
```

## Alerts

The alerts extension parses a GitHub-flavored `> [!KIND]` token as the first line of a blockquote and replaces the `QuoteBlock` with an `AlertBlock` carrying two CSS classes. Pennington registers its own `CustomAlertInlineParser` ahead of Markdig's built-in alert parser; the blockquote form is the only accepted syntax.

### Syntax

````markdown
> [!NOTE]
> Body text of the alert, CommonMark rendered.
````

`KIND` is a case-insensitive alphabetic token; unrecognized kinds still parse and emit a `markdown-alert-<kind>` class using the lowercased token.

### Arguments

Alerts take no arguments; the kind token is the only variable and is drawn from the set listed under built-in kinds.

### Built-in kinds and emitted CSS classes

Every alert receives two classes: `markdown-alert` (constant) and `markdown-alert-<kind>` (derived from the lowercased token). The five built-in GitHub-compatible kinds and their emitted secondary classes are listed below.

| Kind token | Secondary class | Typical use |
|---|---|---|
| `NOTE` | `markdown-alert-note` | Supplementary information. |
| `TIP` | `markdown-alert-tip` | Helpful aside. |
| `CAUTION` | `markdown-alert-caution` | Risky operation — consequences before action. |
| `WARNING` | `markdown-alert-warning` | Something likely to go wrong. |
| `IMPORTANT` | `markdown-alert-important` | Must-read information. |

Backing parser and class-emission logic:

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CustomAlertInlineParser
```

### Minimal example

Markdown excerpt from the DocSite authoring example showing a `[!NOTE]` block in context:

```csharp:xmldocid,bodyonly
M:DocSiteAuthorExample.Stage2.Source
```

## Code annotations

After syntax highlighting, `CodeTransformer` scans each rendered line for a `[!code …]` directive inside a language-appropriate comment, strips the comment, and applies a CSS class to the line and optionally to the enclosing `<pre>`. The `word:` variant wraps a matching substring in a span rather than acting on the whole line; the `include-start` / `include-end` / `exclude-start` / `exclude-end` directives remove surrounding lines from the output.

### Syntax

````markdown
```csharp
var x = 1; // [!code highlight]
var y = 2; // [!code ++]
var z = 3; // [!code word:z|renamed from q]
```
````

The directive must appear inside a recognized comment marker for the language (`//`, `#`, `--`, `<!-- -->`, `*`, `%`, `'`, `REM`, `;`, `/* */`); the directive and any now-empty comment wrapper are removed, leaving trailing content intact.

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

Backing transformer:

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CodeTransformer
```

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

`xref:` links authored in markdown are resolved after rendering by `XrefHtmlRewriter` against the uid-to-route map owned by `XrefResolver`. Two surface forms are supported: the tag form `<xref:uid>` is handled in a pre-parse string pass (it is not valid HTML), and the attribute form `[text](xref:uid)` has its rendered `href="xref:uid"` rewritten during the DOM pass; unknown uids emit a diagnostic via `DiagnosticContext` and surface in the dev overlay.

### Syntax

````markdown
See <xref:reference.options.pennington-options>.

See [PenningtonOptions](xref:reference.options.pennington-options).
````

`uid` is the exact string declared in a page's front-matter `uid:` key.

### Arguments

The only variable is the `uid` token. The tag form derives its link text from the target page's title; the attribute form uses the supplied link text verbatim.

### Emitted CSS classes

The rewriter emits a standard `<a href="…">` element with no added class; styling is delegated to the surrounding prose stylesheet.

### Minimal example

Both surface forms resolving the same uid — the tag form derives its link text from the target page title, the attribute form uses the supplied label verbatim:

```markdown
See <xref:reference.options.pennington-options> for the full options catalog.

Configure MonorailCSS through [the options record](xref:reference.options.monorail-css-options).
```

Backing rewriter:

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

## See also

- How-to: [Tabbed code](xref:how-to.content-authoring.tabbed-code)
- How-to: [Alerts](xref:how-to.content-authoring.alerts)
- How-to: [Code annotations](xref:how-to.content-authoring.code-annotations)
- How-to: [Cross-references](xref:how-to.content-authoring.cross-references)
- Related reference: [Code-block argument reference](xref:reference.markdown.code-block-args)

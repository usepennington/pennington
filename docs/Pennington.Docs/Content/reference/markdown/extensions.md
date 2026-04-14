---
title: "Markdown extensions catalog"
description: "Every non-CommonMark feature Pennington adds to Markdig in one scannable page — tabs, alerts, code annotations, and cross-reference tags — with syntax, arguments, and a minimal example each."
section: "markdown"
order: 10
tags: []
uid: reference.markdown.extensions
isDraft: true
search: false
llms: false
---

> **In this page.** Every non-CommonMark feature in one scannable page — tabs, alerts, code annotations, cross-reference tags — with syntax, arguments, and a minimal example each.
>
> **Not in this page.** Markdig core syntax (tables, footnotes, etc.) — those follow Markdig's own docs.

## Summary

- _One sentence: what it is._ The set of extensions `MarkdownPipelineFactory.CreateWithExtensions` registers on top of Markdig's `UseAdvancedExtensions` + `UseYamlFrontMatter`.
- _One sentence: where it lives._ Namespace `Pennington.Markdown.Extensions` (plus `Pennington.Roslyn.Preprocessing` for the optional Roslyn package).

## Pipeline registration

```csharp:xmldocid
M:Pennington.Markdown.MarkdownPipelineFactory.CreateWithExtensions(Pennington.Highlighting.HighlightingService,System.Func{Pennington.Markdown.Extensions.CodeHighlightRenderOptions},System.Func{Pennington.Markdown.Extensions.Tabs.TabbedCodeBlockRenderOptions},System.Collections.Generic.IEnumerable{Pennington.Markdown.Extensions.ICodeBlockPreprocessor})
```

## Extensions catalog

| # | Extension | Kind | Implementing type |
|---|---|---|---|
| 1 | GitHub-style alerts | Block (inside `>` quote) | `CustomAlertInlineParser` |
| 2 | Tabbed code blocks | Fenced-block group | `TabbedCodeBlocksExtension` |
| 3 | Code annotations | Per-line directives | `CodeTransformer` |
| 4 | Xref cross-reference links | Inline / raw tag | `XrefHtmlRewriter` + `XrefResolvingService` |
| 5 | Roslyn fence preprocessors (optional) | Fenced-block preprocessor | `RoslynCodeBlockPreprocessor` |

---

### 1. GitHub-style alerts

_Quote-block opens with a bracketed keyword on its first line. The keyword becomes the alert kind (lowercased) as a CSS class on the rendered block. Parsed by `CustomAlertInlineParser`; rendered by Markdig's default `AlertBlockRenderer` via `MarkdownPipelineFactory.UseCustomAlerts`._

- **Syntax.** `> [!KEYWORD]` on the first line of a block-quote, then the alert body on subsequent `>` lines.
- **Arguments.** None.
- **Output.** `<div class="markdown-alert markdown-alert-{kind-lowercase}">…</div>` containing a `<p class="markdown-alert-title"><svg …></svg>{Kind}</p>` title element.
- **Constraint.** The marker must be the first inline of the first paragraph inside a `>` blockquote — anything before it causes the parser to bail and the blockquote renders as a normal quote. The kind token is matched by `StringSlice.IsAlpha` (letters only; no digits or hyphens). The blockquote must not already be an `AlertBlock` (re-entry guard in `CustomAlertInlineParser.Match`).

#### Kinds

Any alpha kind is accepted; only the five below carry built-in MonorailCSS styling. Unstyled kinds render as a bare `.markdown-alert` wrapper with no color theme.

| Kind | Syntax | Emitted CSS class | Default icon | Color theme |
|---|---|---|---|---|
| `NOTE` | `> [!NOTE]` | `markdown-alert markdown-alert-note` | GitHub `info` octicon (filled circle with `i`) | Emerald (`fill-emerald-700 … bg-emerald-100/75 …`) |
| `TIP` | `> [!TIP]` | `markdown-alert markdown-alert-tip` | GitHub `light-bulb` octicon | Blue (`fill-blue-700 … bg-blue-100/75 …`) |
| `CAUTION` | `> [!CAUTION]` | `markdown-alert markdown-alert-caution` | GitHub `stop` octicon | Amber (`fill-amber-700 … bg-amber-100/75 …`) |
| `WARNING` | `> [!WARNING]` | `markdown-alert markdown-alert-warning` | GitHub `alert` octicon (triangle with `!`) | Rose (`fill-rose-700 … bg-rose-100/75 …`) |
| `IMPORTANT` | `> [!IMPORTANT]` | `markdown-alert markdown-alert-important` | GitHub `report` octicon | Sky (`fill-sky-700 … bg-sky-100/75 …`) |

Icons come from Markdig's `AlertBlockRenderer.DefaultRenderKind`. The MonorailCSS style table lives at `src/Pennington.MonorailCss/MonorailCssOptions.cs` (lines 477-485).

#### Shared styles

- `.markdown-alert` → `my-6 px-4 flex flex-row gap-2.5 rounded-2xl border text-sm items-center`.
- `.markdown-alert a` → `underline`.
- `.markdown-alert-title` → `text-[0px]` (hides the kind label text but preserves the icon).
- `.markdown-alert svg` → `h-4 w-4 mt-0.5`.

```markdown
> [!NOTE]
> Pennington ships five conventional alert kinds.

> [!WARNING]
> Do not commit `.env` files.
```

---

### 2. Tabbed code blocks

_Two or more consecutive fenced code blocks where the first block carries `tabs=true` in its info-string collapse into one ARIA tablist._

- **Syntax.** Info-string `key=value` pairs after the language, e.g. ` ```csharp tabs=true title="Program.cs" `.
- **Arguments (on the first block — activates tab grouping).**
    - `tabs=true` — required; marks the start of a tab group. Subsequent consecutive fenced blocks join the group.
- **Arguments (on every block in the group — labels the tab).**
    - `title="…"` — optional; tab button label. Falls back to the language's display name (see `LanguageNormalizer`).
- **Value quoting.** Unquoted for single-token values (`tabs=true`); single or double quotes for values with spaces (`title="My File"`).
- **Output.** `<div role="tablist">` with `<button role="tab">` children and sibling `<div>` tab panels. Class names come from `TabbedCodeBlockRenderOptions` (defaults: `tab-container`, `tab-list`, `tab-button`, `tab-panel`).

````markdown
```csharp tabs=true title="Program.cs"
Console.WriteLine("Hello");
```

```json title="appsettings.json"
{ "Logging": { "LogLevel": { "Default": "Information" } } }
```
````

---

### 3. Code annotations (per-line directives)

_A directive is placed inside a language-appropriate comment on its own line (or at end-of-line). `CodeTransformer` parses each line after highlighting, removes the directive, and applies CSS classes to the `<span class="line">`._

- **Syntax.** `<comment-marker> [!code NOTATION]` — e.g. `// [!code highlight]`, `# [!code ++]`, `<!-- [!code focus] -->`.
- **Comment markers recognized.** `//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, `/*` (block-comment endings `*/` and `-->` are also consumed).

#### Line notations

| Notation | Line CSS class added | `<pre>` marker class added |
|---|---|---|
| `highlight` (alias `hl`) | `highlight` | `has-highlighted` |
| `++` | `diff-add` | `has-diff` |
| `--` | `diff-remove` | `has-diff` |
| `focus` | `focused` (all other lines get `blurred`) | `has-focused` |
| `error` | `error` | `has-errors` |
| `warning` | `warning` | `has-warnings` |
| `word:TEXT` | wraps first occurrence of `TEXT` in `<span class="word-highlight">` | `has-word-highlights` |
| `word:TEXT|Message` | as above but class `word-highlight-with-message`, plus a `<div class="word-highlight-message">` callout | `has-word-highlights` |

#### Snippet-region notations

_Regions delete lines from the output. Useful when embedding a slice of a longer source file._

| Notation | Effect |
|---|---|
| `include-start` / `include-end` | Only lines strictly between the markers are kept; all others are removed. One include region per block. |
| `exclude-start` / `exclude-end` | Lines between the markers (inclusive of the marker lines) are removed. One exclude region per block. |

_Note._ The grammars `{1,3}` and `{+1}` are **not** supported — only the `[!code …]` directive form above.

````markdown
```csharp
var x = 1;        // [!code highlight]
var y = 2;        // [!code --]
var z = 3;        // [!code ++]
var keep = "me";  // [!code focus]
var err = bad();  // [!code error]
```
````

---

### 4. Xref cross-reference links

_Two surface syntaxes both resolve to a canonical URL via `XrefResolver`; unknown uids emit a `DiagnosticSeverity.Warning` and render with `data-xref-error`._

| Syntax | Phase | When to use |
|---|---|---|
| `<xref:uid>` | Pre-parse regex pass (`ResolveXrefTagsAsync`) — runs **before** HTML parsing because the tag is not valid HTML. Link text defaults to the target's `Title`. | Self-closing reference; no custom link text. |
| `[link text](xref:uid)` | DOM pass (`ResolveXrefLinksAsync`) on the parsed document. Link text is preserved as written. | Custom link text. |

- **Arguments.** None. The `uid` is the value after `xref:`.
- **Output (resolved).** `<a href="{canonical-path}">{text}</a>`.
- **Output (unresolved).** `<a href="xref:{uid}" data-xref-error="Reference not found" data-xref-uid="{uid}">{uid}</a>` plus a diagnostic.
- **Not supported.** The shape `<xref uid="…">text</xref>` is not recognized — only `<xref:uid>` and `[text](xref:uid)`.

```markdown
See <xref:reference.markdown.extensions> for the full list,
or [the catalog](xref:reference.markdown.extensions) to link with custom text.
```

---

### 5. Roslyn fence preprocessors (optional — `Pennington.Roslyn`)

_Extends the fenced-code info-string with `:modifier` tokens. Registered when `services.AddPenningtonRoslyn(o => o.SolutionPath = …)` is called with a solution path._

| Info-string | Body | Behavior |
|---|---|---|
| ` ```csharp:xmldocid ` | One or more XML doc IDs, one per line (e.g. `T:Namespace.Type`) | Extracts and highlights each symbol's source. |
| ` ```csharp:xmldocid,bodyonly ` | XML doc IDs | As above, but method body only (strips signature / braces). |
| ` ```csharp:xmldocid-diff ` | Exactly two XML doc IDs (one per line) | Renders a line-level DiffPlex diff between the two symbols with `diff-add` / `diff-remove` classes. |
| ` ```csharp:xmldocid-diff,bodyonly ` | Two XML doc IDs | As above, bodies only. |
| ` ```csharp:path ` | A solution-relative file path | Inlines and highlights the file. Rejects `..` and absolute paths. |

- **Language prefix.** `csharp` and `vb` / `vbnet` are detected; `vb` and `vbnet` are routed to the VB highlighter.
- **Failure mode.** Missing symbols / files emit `DiagnosticSeverity.Warning` and render a commented-out error placeholder rather than throwing.

````markdown
```csharp:xmldocid
T:Pennington.Markdown.MarkdownPipelineFactory
```
````

## See also

- Reference: [Front-matter capability interfaces](/reference/front-matter/)
- Reference: [Pennington options](/reference/options/)

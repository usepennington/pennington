---
title: "Add a Markdig extension or inline parser"
description: "Register any Markdig extension, inline parser, or block parser through ConfigureMarkdownPipeline — wiki-links as the worked example — and check what the default pipeline already enables before you add."
uid: how-to.markdown-pipeline.markdig-extension
order: 1
sectionLabel: "Markdown Pipeline"
tags: [extensibility, markdown, markdig, inline-parser]
---

To add syntax Markdig doesn't parse out of the box — `[[wiki-links]]`, a definition shortcut, a custom container — register a Markdig extension or a raw inline/block parser through `PenningtonOptions.ConfigureMarkdownPipeline`. The hook runs after every built-in extension with the resolved `IServiceProvider`, so you extend the same pipeline that renders the rest of the site rather than replacing it.

This is the seam to reach for instead of writing your own renderer. For directives that expand to a string before parsing, a [shortcode](xref:how-to.markdown-pipeline.shortcodes) is lighter; to claim a whole fenced block, use a [code-block preprocessor](xref:how-to.markdown-pipeline.code-block-preprocessor). Everything else — new inline tokens, new block syntax, swapping a renderer — goes through `ConfigureMarkdownPipeline`.

The recipe references `examples/ExtensibilityLabExample/WikiLinkExtension.cs`, which adds a `[[…]]` inline parser to a bare `AddPennington` host.

## Before you begin

- An existing Pennington site with markdown rendering wired (see <xref:tutorials.getting-started.first-site> if not).
- A look at what the pipeline already enables, below — several "missing" features are already on.

## Check what's already enabled

Pennington's pipeline is Markdig's `UseAdvancedExtensions()` plus its own renderers. Before you register anything, confirm the feature isn't already parsed — re-adding an extension that's present is a double-register that, depending on the extension, duplicates parsers, reorders them, or shadows the built-in.

`UseAdvancedExtensions()` enables abbreviations, auto-identifiers (heading `id`s), citations, custom containers, definition lists, emphasis extras, figures, footers, footnotes, grid tables, **mathematics**, media links, pipe tables, list extras, task lists, diagrams, auto links, and generic attributes. On top of that Pennington adds YAML front matter, syntax highlighting, tabbed code blocks, [content tabs](xref:how-to.rich-content.content-tabs), [custom alerts](xref:how-to.rich-content.alerts), horizontally scrollable tables, and Mdazor component rendering. The [extensions catalog](xref:reference.markdown.extensions) documents the Pennington-specific syntax.

The durable way to stay safe is `AddIfNotAlready` — it adds your extension only when an instance of that type isn't already in the pipeline:

```csharp
penn.ConfigureMarkdownPipeline = (pipeline, _) =>
    pipeline.Extensions.AddIfNotAlready(new WikiLinkExtension());
```

## Math already works — don't re-register it

Because `UseAdvancedExtensions()` includes the mathematics extension, math is parsed today with no configuration. Inline `$E = mc^2$` renders to `<span class="math">\(E = mc^2\)</span>` and a `$$…$$` block to `<div class="math">\[…\]</div>` — already in the `\(…\)` / `\[…\]` delimiters KaTeX and MathJax expect. Registering `UseMathematics()` again is the double-register to avoid.

What's missing is the *rendering* of that markup, which is a client-side step, not a Markdig one. Load KaTeX (or MathJax) through the head seam and run its auto-render once per page. On a DocSite that seam is `DocSiteOptions.AdditionalHtmlHeadContent`:

```csharp
options.AdditionalHtmlHeadContent = """
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/katex@0.16.11/dist/katex.min.css">
    <script defer src="https://cdn.jsdelivr.net/npm/katex@0.16.11/dist/katex.min.js"></script>
    <script defer src="https://cdn.jsdelivr.net/npm/katex@0.16.11/dist/contrib/auto-render.min.js"></script>
    <script defer src="/math.js"></script>
    """;
```

`math.js` calls `renderMathInElement` on load and again on `spa:commit`, since the SPA engine swaps page content without a full reload:

```javascript
function typeset() {
  renderMathInElement(document.body, {
    delimiters: [
      { left: "\\(", right: "\\)", display: false },
      { left: "\\[", right: "\\]", display: true },
    ],
  });
}
document.addEventListener("DOMContentLoaded", typeset);
document.addEventListener("spa:commit", typeset);
```

This is the same wiring as any browser library that upgrades server-rendered markup — see <xref:how-to.rich-content.client-side-widget> for the full pattern and <xref:how-to.rich-content.diagrams> for the bundled Mermaid version of it. On a bare `AddPennington` host, inject the same tags through your own layout's `<head>` or a <xref:how-to.response-pipeline.response-processor>.

## Write a custom inline parser

A genuinely new token needs a parser. `WikiLinkExtension` is an `IMarkdownExtension` whose `Setup` inserts one inline parser; the parser claims `[[Target]]` / `[[Target|Label]]` and emits an ordinary `LinkInline` so Markdig's own anchor renderer writes the `<a>`.

```csharp:symbol
examples/ExtensibilityLabExample/WikiLinkExtension.cs
```

Three details carry the parser:

- **`OpeningCharacters` triggers `Match`.** The parser registers `[`, then returns `false` immediately unless the next character is also `[`, leaving single-bracket `[text](url)` links to the built-in parser.
- **Insert before the link parser.** `InsertBefore<LinkInlineParser>` gives the wiki-link parser first claim on `[[`; otherwise the CommonMark link parser reads the brackets as nested links.
- **Emit a `LinkInline` and tag it.** Setting `Url`, appending a `LiteralInline` label, and calling `GetAttributes().AddClass("wikilink")` produces a normal anchor with a class downstream code can target. Restore the saved `StringSlice` and return `false` on any malformed input so other parsers get their turn.

A block-level construct follows the same shape with a `BlockParser` inserted into `pipeline.BlockParsers`; to change how an existing node renders, swap its renderer in the second `Setup(MarkdownPipeline, IMarkdownRenderer)` overload the way Pennington's own syntax-highlighting and scrollable-tables extensions replace Markdig's default code-block and table renderers.

## Register it through ConfigureMarkdownPipeline

`ConfigureMarkdownPipeline` is `Action<MarkdownPipelineBuilder, IServiceProvider>`, set inside the `AddPennington` lambda. It runs after the built-ins, and the second argument is the resolved service provider — resolve dependencies from it when a parser needs them (an `HttpClient`, options, a file-watched index). The lab discards it with `_`:

```csharp
penn.ConfigureMarkdownPipeline = (pipeline, _) =>
    pipeline.Extensions.AddIfNotAlready(new WikiLinkExtension());
```

## Result

The demo page uses both wiki-link forms and a math block:

```markdown:symbol
examples/ExtensibilityLabExample/Content/wikilinks-demo.md
```

The wiki-links render as anchors carrying `class="wikilink"`, and the math block renders as the KaTeX-ready `<div class="math">`:

```html
<a href="/notes/glossary/" class="wikilink">Glossary</a>
<a href="/notes/content-pipeline/" class="wikilink">how rendering works</a>

<div class="math">
\[
\int_0^1 x^2 \,\mathrm{d}x = \frac{1}{3}
\]</div>
```

## How your custom HTML survives the response pipeline

After rendering, every page passes through the response rewriters and is harvested by the site projection for search and llms.txt. Custom markup is a first-class citizen in all three, with one thing to watch.

**Response rewriters mutate only what their selectors match.** Each `IHtmlResponseRewriter` runs an AngleSharp query over the parsed document; elements and classes it doesn't target pass through untouched. Internal `href`s you emit *are* rewritten — the shipped rewriters add the locale prefix and the deploy base URL — so a wiki-link to `/notes/glossary/` is portable across locales and sub-path deploys exactly like an authored link. The one to know about: the opt-in [word-break rewriter](xref:how-to.response-pipeline.html-rewriter)'s default selector includes `span`, so if you emit a text-bearing `<span>` that a client script reads verbatim (a math span, say), either leave word-break off for it or narrow its selector.

**The site projection keeps whatever is inside the content region.** Search, llms.txt, and the link audit read the element named by `PenningtonOptions.SiteProjection.ContentSelector` (the lab uses `article`). Emit your custom HTML into the article body and it's captured; HTML injected into the chrome or `<head>` is not. See <xref:how-to.discovery.search> and <xref:how-to.feeds.llms-txt>.

**Search indexes your custom HTML as plain text.** The heading-section extractor walks the rendered content, indexing each section's `textContent` (whitespace-collapsed) and dropping `<pre>` subtrees. So a wiki-link is found by its visible label, and a math span by its LaTeX source; the classes and structure don't reach the index, only the text does. Headings need an `id` to start a section — `UseAdvancedExtensions()` already supplies those.

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/wikilinks-demo/`. View source: the wiki-links are `<a class="wikilink" href="/notes/…/">` and the math block is `<div class="math">`.
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build` — grep `output/wikilinks-demo/index.html` for `class="wikilink"` and `<div class="math">`. The build also reports the `/notes/…/` links as broken, since the lab has no `/notes/` pages — the build-time link audit treats your custom anchors as real internal links, exactly as intended.

## Related

- How-to: [Expand a directive before Markdig parses](xref:how-to.markdown-pipeline.shortcodes) — string expansion before the parser runs, for stamping values rather than new syntax.
- How-to: [Add a custom fence syntax](xref:how-to.markdown-pipeline.code-block-preprocessor) — claim a fenced block instead of an inline token.
- How-to: [Ship a custom client-side widget](xref:how-to.rich-content.client-side-widget) — the head-seam + `spa:commit` pattern the math example uses.
- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the syntax the default pipeline already provides.
- Background: [The response-processing pipeline](xref:explanation.core.response-processing) — where the rewriters and projection run.

---
title: "Add a Markdig extension or inline parser"
description: "Register any Markdig extension, inline parser, or block parser through ConfigureMarkdownPipeline — wiki-links as the worked example — and check what the default pipeline already enables before you add."
uid: how-to.markdown-pipeline.markdig-extension
order: 1
sectionLabel: "Markdown Pipeline"
tags: [extensibility, markdown, markdig, inline-parser]
---

To add syntax Markdig doesn't parse out of the box — `[[wiki-links]]`, a definition shortcut, a custom container — register a Markdig extension or a raw inline/block parser through `PenningtonOptions.ConfigureMarkdownPipeline`. The hook runs after every built-in extension with the resolved `IServiceProvider`, so you extend the same pipeline that renders the rest of the site rather than replacing it.

This is the hook to reach for instead of writing your own renderer. For directives that expand to a string before parsing, a [shortcode](xref:how-to.markdown-pipeline.shortcodes) is lighter; to claim a whole fenced block, use a [code-block preprocessor](xref:how-to.markdown-pipeline.code-block-preprocessor). Everything else — new inline tokens, new block syntax, swapping a renderer — goes through `ConfigureMarkdownPipeline`.

The recipe references `examples/ExtensibilityLabExample/WikiLinkExtension.cs`, which adds a `[[…]]` inline parser to a bare `AddPennington` host.

## Before you begin

- An existing Pennington site with markdown rendering wired (see <xref:tutorials.getting-started.first-site> if not).
- A look at what the pipeline already enables, below — several "missing" features are already on.

## Check what's already enabled

Pennington's pipeline is Markdig's `UseAdvancedExtensions()` plus its own renderers. Before you register anything, confirm the feature isn't already parsed — re-adding an extension that's present is a double-register that, depending on the extension, duplicates parsers, reorders them, or shadows the built-in.

`UseAdvancedExtensions()` already turns on the usual advanced set — auto-identifiers, footnotes, grid and pipe tables, task lists, **mathematics**, and the rest — and Pennington layers its own front matter, syntax highlighting, tabbed code, [content tabs](xref:how-to.rich-content.content-tabs), [custom alerts](xref:how-to.rich-content.alerts), and Mdazor rendering on top. The [extensions catalog](xref:reference.markdown.extensions) is the full list to check against.

## Math already works — don't re-register it

`UseAdvancedExtensions()` includes the mathematics extension, so math is parsed today with no configuration. Inline `$E = mc^2$` renders to `<span class="math">\(E = mc^2\)</span>` and a `$$…$$` block to `<div class="math">\[…\]</div>` — already in the `\(…\)` / `\[…\]` delimiters KaTeX and MathJax expect. Registering `UseMathematics()` again is the double-register to avoid. Turning that markup into typeset math is a client-side step, not a Markdig one: load KaTeX (or MathJax) through the head content option and re-run its auto-render on `spa:commit`, exactly the head-content-plus-script pattern in <xref:how-to.rich-content.client-side-widget>.

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

After rendering, every page passes through the response rewriters and is harvested by the site projection. Custom markup rides through the same as built-in markup, with two things to do.

**Emit your HTML into the content region.** Search, llms.txt, and the link audit read the element named by `PenningtonOptions.SiteProjection.ContentSelector` (the lab uses `article`); markup placed there is captured and indexed by its visible text, while anything injected into the chrome or `<head>` is not. See <xref:how-to.discovery.search> and <xref:how-to.feeds.llms-txt>.

**Watch the word-break rewriter if you emit text-bearing `<span>`s.** The shipped rewriters rewrite the internal `href`s you emit — adding the locale prefix and deploy base URL, so a wiki-link to `/notes/glossary/` stays portable — but the opt-in [word-break rewriter](xref:how-to.response-pipeline.html-rewriter)'s default selector includes `span`. If a client script reads a `<span>` verbatim (a math span, say), leave word-break off for it or narrow its selector.

## Verify

On your own site, register your extension through `ConfigureMarkdownPipeline`, put a `[[Target]]` (or your token) in any content page, run your site, and view source on that page: your token rendered as the markup your parser emits — `<a class="wikilink" href="/notes/…/">` for the wiki-link parser. Then run your static build and grep the page's `index.html` in the output for the same markup. The build-time link audit treats custom anchors as real internal links, so it reports any `href` with no matching page as broken — which is the audit working, not a misfire.

To check against the reference implementation, run `dotnet run --project examples/ExtensibilityLabExample`, visit `/wikilinks-demo/`, and confirm the wiki-links are `<a class="wikilink" href="/notes/…/">` and the math block is `<div class="math">`.

## Related

- How-to: [Expand a directive before Markdig parses](xref:how-to.markdown-pipeline.shortcodes) — string expansion before the parser runs, for stamping values rather than new syntax.
- How-to: [Add a custom fence syntax](xref:how-to.markdown-pipeline.code-block-preprocessor) — claim a fenced block instead of an inline token.
- How-to: [Ship a custom client-side widget](xref:how-to.rich-content.client-side-widget) — the head-content + `spa:commit` pattern the math example uses.
- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the syntax the default pipeline already provides.
- Background: [The response-processing pipeline](xref:explanation.core.response-processing) — where the rewriters and projection run.

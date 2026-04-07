---
title: "Syntax Highlighting System"
description: "Penn's pluggable, priority-based syntax highlighting architecture from Markdig integration to Roslyn-powered semantic highlighting"
uid: "penn.under-the-hood.syntax-highlighting-system"
order: 3020
---

Penn does all syntax highlighting at build time. There is no client-side JavaScript highlighter, no flash of unstyled code, and no runtime cost per page view. The system is built around a priority-based dispatch model where multiple highlighters coexist and the best one for each language wins automatically. Register a new highlighter or preprocessor and it slots into the pipeline by priority with zero configuration changes.

## The Full Stack at a Glance

Here is the path a fenced code block takes from Markdown source to final HTML:

```
Markdown fenced code block
  -> CodeHighlightRenderer (Markdig extension)
       -> ICodeBlockPreprocessor[] (checked first, highest priority wins)
       -> HighlightingService (priority dispatch)
            -> RoslynHighlighter (100), ShellHighlighter (75),
               TextMateHighlighter (50), PlainTextHighlighter (0)
       -> CodeTransformer (line annotations)
       -> CodeBlockHtmlBuilder (final HTML wrapper)
```

`CodeHighlightRenderer` is a Markdig `HtmlObjectRenderer<CodeBlock>` that intercepts fenced code blocks during Markdown rendering. It first offers the block to any registered `ICodeBlockPreprocessor` implementations, which can handle special modifiers like `:xmldocid` or `:path`. If no preprocessor claims the block, `HighlightingService` dispatches to the highest-priority `ICodeHighlighter` that supports the requested language. `CodeTransformer` then processes any `[!code ...]` line-annotation directives embedded in comments, adding CSS classes for highlighting, diffs, focus, and errors. Finally, `CodeBlockHtmlBuilder` wraps the result in a standard div structure with configurable CSS classes.

## How a Code Block Gets Highlighted

The `CodeHighlightRenderer.Write()` method runs a six-step sequence for every fenced code block in the Markdown AST.

**Step 1 -- Extract language and code.** `TryExtractFencedCodeBlock` pulls the language identifier and raw code text from the Markdig `FencedCodeBlock` node. `ParseBaseLanguage` splits the language string at the first colon, so `csharp:xmldocid` yields base language `csharp` and modifier `xmldocid`. The raw code text is reassembled from the Markdig line-slice array with trailing whitespace trimmed.

**Step 2 -- Check preprocessors.** The renderer iterates through `ICodeBlockPreprocessor[]` in descending priority order. The first preprocessor that returns a non-null `CodeBlockPreprocessResult` wins. If the result's `SkipTransform` flag is false, the preprocessed HTML still passes through `CodeTransformer` for line annotations. If `SkipTransform` is true, the HTML goes directly to the builder. When a preprocessor handles a block, the normal highlighting path is skipped entirely.

**Step 3 -- Highlight via HighlightingService.** If no preprocessor claimed the block, `HighlightingService.Highlight(code, baseLanguage)` dispatches to the highest-priority highlighter that supports the language. The service returns an HTML string with `<pre><code>` wrappers and `<span>` elements for syntax tokens.

**Step 4 -- Transform.** `CodeTransformer.Transform()` processes `[!code ...]` directives embedded in code comments. This step is skipped for blocks with a `markdown` or `md` language identifier, since those blocks may legitimately contain `[!code ...]` text as content.

**Step 5 -- Wrap.** `CodeBlockHtmlBuilder.BuildHtml()` adds outer `<div>` elements with configurable CSS classes. The builder is aware of whether the block sits inside a tabbed code group (`isInTabGroup`), and omits the standalone container div for tabbed blocks.

**Step 6 -- Error fallback.** If any exception occurs during steps 3 through 5, the renderer catches it and emits HTML-encoded plain text inside the standard wrapper. The page still renders; only the highlighting is lost.

## ICodeHighlighter: The Priority Contract

The `ICodeHighlighter` interface defines the contract that every highlighter must satisfy:

```csharp:xmldocid
T:Penn.Highlighting.ICodeHighlighter
```

Three members drive the dispatch model:

- **`SupportedLanguages`** -- a read-only set of language identifiers this highlighter handles. The string `"*"` serves as a wildcard, meaning the highlighter accepts any language.
- **`Highlight(code, language)`** -- accepts raw code and a language identifier, returns an HTML string with `<pre><code>` wrappers and `<span>` elements carrying CSS classes for syntax tokens.
- **`Priority`** -- an integer where higher values win. When multiple highlighters support the same language, the one with the highest priority is selected. This is the sole dispatch mechanism.

`HighlightingService` orchestrates the dispatch:

```csharp:xmldocid
T:Penn.Highlighting.HighlightingService
```

At construction time, the service sorts all injected `ICodeHighlighter` instances by priority descending. `FindHighlighter` iterates the sorted list and returns the first highlighter whose `SupportedLanguages` contains the requested language or the `"*"` wildcard. If nothing matches -- which should not happen when TextMate is registered -- the service falls back to an internal `PlainTextHighlighter` instance.

The `HasHighlighter(language)` method checks whether any registered highlighter (excluding the fallback) supports a given language, which is useful for conditional rendering logic without invoking the full highlighting path.

## The Highlighter Stack

Penn ships with three built-in highlighters and supports an optional fourth from the Penn.Roslyn package:

| Highlighter | Priority | Languages | Package |
|---|---|---|---|
| `RoslynHighlighter` | 100 | `csharp`, `cs`, `c#`, `vb`, `vbnet` | Penn.Roslyn |
| `ShellHighlighter` | 75 | `bash`, `shell`, `sh` | Penn |
| `TextMateHighlighter` | 50 | `*` (all TextMate grammars) | Penn |
| `PlainTextHighlighter` | 0 | `*` (universal fallback) | Penn |

### ShellHighlighter

The `ShellHighlighter` exists because TextMate shell grammars produce inconsistent results across common shell patterns. This dedicated highlighter uses compiled regular expressions to tokenize shell commands into four categories: commands (the first word on each line), flags and options (`-f`, `--verbose`), quoted strings, and comments (`#` and `REM`). It maps each token category to the `hljs-*` CSS class convention (`hljs-built_in` for commands, `hljs-params` for flags, `hljs-string` for strings, `hljs-comment` for comments). At priority 75, it beats TextMate for `bash`, `shell`, and `sh` language identifiers while letting TextMate handle everything else.

### TextMateHighlighter

The `TextMateHighlighter` uses TextMateSharp to load VS Code grammar definitions, giving Penn access to syntax grammars for dozens of languages out of the box. It tokenizes code line by line, then maps TextMate scopes to `hljs-*` CSS classes via a static scope-mapping table. The table is ordered from most-specific to least-specific -- `comment.line.double-slash` is checked before the broader `comment` scope, and `entity.name.function` before the catch-all `entity`. When resolving a grammar, the highlighter first tries the `TextMateLanguageRegistry` lookup, then falls back to a `source.{language}` scope pattern.

The TextMateSharp registry is not thread-safe, so all tokenization runs under a lock. This is acceptable for build-time use where throughput matters less than correctness. A five-second per-line tokenize time limit prevents runaway grammars from blocking the build.

`SupportedLanguages` is set to `{"*"}`, making TextMate the catch-all highlighter for any language not claimed by a higher-priority implementation.

### PlainTextHighlighter

The `PlainTextHighlighter` is the universal fallback at priority 0. It also declares `SupportedLanguages` as `{"*"}`, but its lower priority means it only activates when no other highlighter matches. It HTML-encodes the code and returns it directly -- no `<pre><code>` wrapping, no syntax spans.

### RoslynHighlighter (Optional)

The `RoslynHighlighter` wraps the `SyntaxHighlighter` from Penn.Roslyn and registers at priority 100, the highest in the stack. It uses Roslyn's Classifier API via an `AdhocWorkspace` to perform semantic classification -- `var` is identified as a keyword, `List<T>` as a type, `Console.WriteLine` as a method call. This produces more accurate highlighting than any grammar-based approach can achieve. See <xref:penn.getting-started.connecting-to-roslyn> for setup and configuration details.

## ICodeBlockPreprocessor: Intercepting Before Highlighting

Preprocessors intercept code blocks before they reach `HighlightingService`. They handle blocks that carry special language modifiers and need non-standard processing.

```csharp:xmldocid
T:Penn.Markdown.Extensions.ICodeBlockPreprocessor
```

The interface follows the same priority model as `ICodeHighlighter`: implementations are sorted by `Priority` descending, and the first one that returns a non-null `CodeBlockPreprocessResult` wins. The result record carries `HighlightedHtml` (the fully highlighted HTML wrapped in `pre`/`code` tags), `BaseLanguage` (for CSS class purposes), and `SkipTransform` (whether to bypass `CodeTransformer`).

### RoslynCodeBlockPreprocessor

The Penn.Roslyn package ships a preprocessor at priority 100 that handles three language modifiers:

- **`:xmldocid`** -- the code block body contains one or more XML documentation IDs (one per line). The preprocessor resolves each ID against the Roslyn workspace, extracts the corresponding source code, and highlights it with Roslyn's classifier. The `,bodyonly` suffix (e.g., `csharp:xmldocid,bodyonly`) extracts only the method body, stripping the signature and braces.
- **`:path`** -- the code block body contains a file path relative to the solution root. The preprocessor reads the file, validates against directory traversal (`..` and rooted paths are rejected), and highlights the contents with Roslyn.
- **`:xmldocid-diff`** -- the code block body contains exactly two XML documentation IDs. The preprocessor extracts and highlights both symbols, then computes a line-level diff using DiffPlex. Lines present only in the first symbol get the `diff-remove` class; lines only in the second get `diff-add`. The result has `SkipTransform` set to true because the diff classes are already applied.

See <xref:penn.getting-started.connecting-to-roslyn> for full Roslyn workspace configuration.

## CodeTransformer: Line Annotations

`CodeTransformer` processes `[!code ...]` directives embedded inside code comments. These directives control per-line visual styling without affecting the displayed code. The transformer strips each directive from the output and applies the corresponding CSS class to the line's `<span>` element.

The following directives are supported:

| Directive | CSS Class | Effect |
|---|---|---|
| `// [!code highlight]` | `highlight` | Highlights the line |
| `// [!code hl]` | `highlight` | Alias for highlight |
| `// [!code ++]` | `diff-add` | Shows line as added |
| `// [!code --]` | `diff-remove` | Shows line as removed |
| `// [!code focus]` | `focused` | Focuses line; others get `blurred` |
| `// [!code error]` | `error` | Marks line as error |
| `// [!code warning]` | `warning` | Marks line as warning |
| `// [!code word:term]` | `word-highlight` | Highlights a specific word on the line |
| `// [!code word:term\|msg]` | `word-highlight-with-message` | Word highlight with popup callout |

The comment marker is language-agnostic. `CodeTransformer` recognizes `//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, and `/*` as valid comment prefixes for directives.

Processing happens in six stages:

1. **`StructureCodeIntoLines`** restructures the `<code>` element's innerHTML into individual `<span class="line">` elements, one per source line. This gives the transformer a per-line DOM structure to manipulate.

2. **`FindDirective`** scans each line's text content for `[!code ...]` inside a recognized comment marker. It returns a `DirectiveMatch` containing the full matched text, the notation (e.g., `highlight`, `++`, `word:term`), and position indices.

3. **Directive removal** strips the directive comment from the HTML DOM. If the directive is the only content in a comment, the entire comment is removed. If other code follows the comment marker, the marker is preserved and only the directive portion is stripped. Orphaned empty spans are cleaned up, and adjacent spans with the same CSS class are merged.

4. **Snippet directives** -- `include-start`/`include-end` and `exclude-start`/`exclude-end` -- operate at the region level. Include regions keep only the lines between the markers and remove everything else. Exclude regions remove the lines between the markers and keep everything else. After line removal, all transformation line numbers are re-indexed to account for the removed lines.

5. **`ApplyTransformationsToDom`** adds CSS classes to line `<span>` elements based on the parsed directives. Focus mode adds a `has-focused` class to the `<pre>` element and a `blurred` class to every non-focused line. Diff directives add `has-diff` to `<pre>`. Error and warning directives add `has-errors` and `has-warnings` respectively.

6. **`NormalizeLineIndents`** strips common leading whitespace across all remaining lines. This is particularly useful after snippet extraction, where included regions may carry indentation from their original position in the source file.

See <xref:penn.guides.markdown-extensions> for usage examples of each directive.

## CodeBlockHtmlBuilder: The Final Wrapper

`CodeBlockHtmlBuilder` wraps highlighted code in a consistent HTML structure. The default output looks like this:

```html
<div class="code-highlight-wrapper not-prose">
  <div class="standalone-code-container">
    <div class="standalone-code-highlight">
      <pre><code>...highlighted code...</code></pre>
    </div>
  </div>
</div>
```

The CSS classes are configurable through `CodeHighlightRenderOptions`, which exposes four properties: `OuterWrapperCss` (the outermost div), `StandaloneContainerCss` (the container div), `PreBaseCss` (always applied to the pre wrapper div), and `PreStandaloneCss` (applied to the pre wrapper div only when the block is not inside a tab group). When a code block is inside a tabbed code group (`isInTabGroup = true`), the standalone container div is omitted entirely, since the tab component provides its own container structure.

## Adding a Custom Highlighter

Implement `ICodeHighlighter` and register it with dependency injection:

```csharp
services.AddSingleton<ICodeHighlighter, MyCustomHighlighter>();
```

Set `Priority` higher than 50 to beat TextMate for your target languages. Keep `SupportedLanguages` specific to the languages you handle -- do not register `"*"` unless your highlighter genuinely handles every language.

The same pattern applies to custom preprocessors. Implement `ICodeBlockPreprocessor`, register it as a singleton, and set its `Priority` to control where it falls in the evaluation order. A preprocessor that returns a non-null `CodeBlockPreprocessResult` will short-circuit all other preprocessors and the normal highlighting path for that block.

## Why Server-Side?

Four properties of server-side highlighting make it the right fit for documentation sites.

**No FOUC.** Code blocks arrive in the browser fully highlighted. There is no moment where raw text is visible before a JavaScript highlighter processes it.

**Smaller payloads.** No highlight.js, Prism, or Shiki bundles are shipped to the client. The only cost is the CSS classes already present in the stylesheet.

**Semantic accuracy.** Roslyn uses the actual C# compiler to classify tokens. `var` is a keyword, `List<T>` is a type name, `Console.WriteLine` is a method invocation. Grammar-based highlighters guess at these distinctions; Roslyn knows.

**Consistency.** The same highlighting runs during `dotnet watch` development and in the production static build. What you see in the browser during authoring is what gets deployed.

The trade-off is build-time cost. Roslyn loads the full solution workspace, which adds seconds to the initial build. For a build-once, serve-many documentation site, this is the right trade-off. The TextMate and Shell highlighters carry no such cost and remain available when Roslyn is not configured.

## Next Steps

- <xref:penn.getting-started.connecting-to-roslyn> -- set up Penn.Roslyn for semantic C#/VB highlighting
- <xref:penn.guides.markdown-extensions> -- usage examples for all code annotation directives

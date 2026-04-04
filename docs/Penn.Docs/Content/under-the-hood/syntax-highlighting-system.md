---
title: "Syntax Highlighting System"
description: "Penn's pluggable, priority-based syntax highlighting architecture from Markdig integration to Roslyn-powered semantic highlighting"
uid: "penn.under-the-hood.syntax-highlighting-system"
order: 3020
---

Penn does all its syntax highlighting at build time. No client-side JavaScript, no runtime cost for readers, no flash of unstyled code. The system is pluggable, priority-based, and -- thanks to the optional Penn.Roslyn package -- can produce semantically accurate highlighting for C# and VB.NET using the actual compiler.

Here is the full stack, top to bottom:

```
Markdown fenced code block
  |
  v
CodeHighlightRenderer (Markdig extension)
  |
  +-- ICodeBlockPreprocessor[] (checked first, highest priority wins)
  |     |
  |     +-- RoslynCodeBlockPreprocessor (priority 100)
  |           handles :xmldocid, :path, :xmldocid-diff modifiers
  |
  +-- HighlightingService (priority dispatch for normal code blocks)
  |     |
  |     +-- RoslynHighlighter    (priority 100, C#/VB)  [Penn.Roslyn, optional]
  |     +-- ShellHighlighter     (priority 75, bash/shell/sh)
  |     +-- TextMateHighlighter  (priority 50, all TextMate grammars)
  |     +-- PlainTextHighlighter (priority 0, fallback)
  |
  +-- CodeTransformer (line annotations: highlight, diff, focus, etc.)
  |
  +-- CodeBlockHtmlBuilder (final HTML wrapper)
```

## How a Code Block Gets Highlighted

When Markdig encounters a fenced code block, it delegates rendering to `CodeHighlightRenderer`, which replaces the default Markdig `CodeBlockRenderer`. Here is the sequence:

1. **Extract language and code.** The renderer pulls the language identifier (e.g., `csharp`, `bash`, `python:xmldocid`) and the raw code text from the Markdig AST.

2. **Check preprocessors.** Each registered `ICodeBlockPreprocessor` gets a shot at the block, in priority order (highest first). If a preprocessor returns a `CodeBlockPreprocessResult`, the renderer uses that HTML and skips normal highlighting. If all preprocessors return `null`, we continue.

3. **Parse language modifiers.** The language identifier is split at the first colon: `csharp:xmldocid` becomes base language `csharp` with modifier `xmldocid`. Plain `python` has no modifier.

4. **Dispatch to HighlightingService.** The service finds the highest-priority `ICodeHighlighter` that supports the base language and calls `Highlight(code, language)`.

5. **Apply line transformations.** `CodeTransformer.Transform()` processes inline directives like `// [!code highlight]`, `// [!code ++]`, and `// [!code focus]`, restructuring the HTML into individual `<span class="line">` elements with appropriate CSS classes.

6. **Wrap in HTML.** `CodeBlockHtmlBuilder.BuildHtml()` adds the outer wrapper divs with configurable CSS classes.

## ICodeHighlighter: The Priority Contract

Every highlighter implements a simple interface:

```csharp:xmldocid
T:Penn.Highlighting.ICodeHighlighter
```

The contract is intentionally minimal:

- **`SupportedLanguages`**: A set of language identifiers this highlighter handles. Use `"*"` to match everything (TextMate and PlainText do this).
- **`Highlight(code, language)`**: Returns HTML with `<pre><code>` wrapping and `<span>` elements for tokens.
- **`Priority`**: An integer. Higher wins.

`HighlightingService` sorts all registered highlighters by priority descending and picks the first match:

```csharp:xmldocid
T:Penn.Highlighting.HighlightingService
```

The dispatch logic is deliberately simple -- iterate the sorted list, return the first highlighter whose `SupportedLanguages` contains the requested language (or `"*"`). If nothing matches at all, `PlainTextHighlighter` provides the fallback. It just HTML-encodes the code and wraps it in `<pre><code>`.

### The Highlighter Stack

| Highlighter | Priority | Languages | Package |
|---|---|---|---|
| `RoslynHighlighter` | 100 | `csharp`, `cs`, `c#`, `vb`, `vbnet` | Penn.Roslyn |
| `ShellHighlighter` | 75 | `bash`, `shell`, `sh` | Penn |
| `TextMateHighlighter` | 50 | `*` (all TextMate grammars) | Penn |
| `PlainTextHighlighter` | 0 | `*` (universal fallback) | Penn |

Because `RoslynHighlighter` has the highest priority for C#, it always wins over TextMate when Penn.Roslyn is installed. Remove the Penn.Roslyn package and TextMate seamlessly takes over -- no configuration changes needed.

### ShellHighlighter

Shell gets its own highlighter because TextMate grammars for shell scripting are... let's say "inconsistent." The `ShellHighlighter` uses regex-based tokenization tuned for command-line documentation:

- Command recognition (first word on each line)
- Flag highlighting (`-f`, `--verbose`)
- String detection (single and double quotes)
- Comment support (`#` and `REM`)

It produces CSS classes compatible with the `hljs-*` convention, same as every other highlighter.

### TextMateHighlighter

`TextMateHighlighter` uses TextMateSharp to load VS Code grammar definitions. It supports 49+ languages including all the usual suspects (JavaScript, Python, Rust, Go, Java, etc.) plus more obscure ones (HLSL, Typst, Clojure).

The highlighter works by tokenizing each line against a TextMate grammar and mapping TextMate scopes to `hljs-*` CSS classes. The scope mapping table covers comments, keywords, strings, types, functions, operators, and many more -- ordered from most specific to least specific so that `comment.line.double-slash` matches before the generic `comment`.

Grammar resolution tries multiple strategies: first the registry's own language-to-scope mapping, then `source.{language}` patterns. If no grammar can be found, it falls back to plain HTML-encoded output.

One quirk: TextMateSharp's registry is not thread-safe, so all tokenization happens under a lock. This is fine for build-time highlighting -- you are not tokenizing code on every page request.

## ICodeBlockPreprocessor: Intercepting Before Highlighting

Preprocessors run *before* the normal highlighting pipeline. They are the escape hatch for code blocks that need special treatment.

```csharp:xmldocid
T:Penn.Markdown.Extensions.ICodeBlockPreprocessor
```

A preprocessor receives the raw code and the full language identifier (including modifiers like `:xmldocid`). It returns a `CodeBlockPreprocessResult` if it handled the block, or `null` to pass through to normal highlighting.

The result includes:

- **`HighlightedHtml`**: The fully highlighted HTML.
- **`BaseLanguage`**: For CSS class purposes.
- **`SkipTransform`**: If `true`, `CodeTransformer` is not applied. Useful when the preprocessor produces its own line structure.

### RoslynCodeBlockPreprocessor

The star preprocessor. It handles three language modifiers:

#### `:xmldocid` -- Symbol Extraction

````markdown
```csharp:xmldocid
T:Penn.Pipeline.ContentItem
```
````

The preprocessor resolves the XML documentation ID against the loaded Roslyn workspace, extracts the full source code of the symbol, highlights it with Roslyn's semantic classifier, and returns the result. Multiple IDs can be listed (one per line) and they are concatenated in the output.

Add `,bodyonly` to strip the containing type and show just the method body:

````markdown
```csharp:xmldocid,bodyonly
M:Penn.Pipeline.ContentPipeline.RunAsync(Penn.Generation.OutputOptions)
```
````

#### `:path` -- File Inclusion

````markdown
```csharp:path
src/Penn/Pipeline/ContentItem.cs
```
````

Reads the file from disk (relative to the solution root), highlights it with Roslyn, and returns the result. Works for any file in the solution, not just C#.

#### `:xmldocid-diff` -- Side-by-Side Comparison

````markdown
```csharp:xmldocid-diff
T:Penn.Pipeline.ContentItem
T:Penn.Pipeline.ContentSource
```
````

Extracts two symbols, highlights both with Roslyn, then computes a line-level diff using DiffPlex. Added lines get `diff-add`, removed lines get `diff-remove`. Requires exactly two XML doc IDs.

## CodeTransformer: Line Annotations

After highlighting, `CodeTransformer` processes inline directives embedded in code comments. These directives control visual presentation without affecting the actual code:

| Directive | CSS Class | Effect |
|---|---|---|
| `// [!code highlight]` | `highlight` | Highlights the line |
| `// [!code hl]` | `highlight` | Alias for highlight |
| `// [!code ++]` | `diff-add` | Shows line as added (diff) |
| `// [!code --]` | `diff-remove` | Shows line as removed (diff) |
| `// [!code focus]` | `focused` | Focuses the line (others get `blurred`) |
| `// [!code error]` | `error` | Marks line as error |
| `// [!code warning]` | `warning` | Marks line as warning |
| `// [!code word:term]` | `word-highlight` | Highlights a specific word |
| `// [!code word:term\|msg]` | `word-highlight-with-message` | Highlights word with callout |

The transformer also handles snippet directives (`[!code include-start]`, `[!code exclude-start]`, etc.) for showing only portions of a larger code block.

The directive comment itself is stripped from the output. If the comment marker is the only thing on the line after stripping, the entire comment is removed. The transformer works across all comment styles -- `//`, `#`, `--`, `<!--`, `*`, `%`, `'`, `REM`, `;`, and `/*`.

The transformer restructures the HTML into individual line `<span>` elements, which enables per-line CSS styling. It also normalizes indentation after snippet extraction, so included code fragments are not awkwardly indented.

## CodeBlockHtmlBuilder: The Final Wrapper

The last step wraps everything in a consistent HTML structure:

```html
<div class="code-highlight-wrapper not-prose">
  <div class="standalone-code-container">
    <div class="standalone-code-highlight">
      <pre><code>...highlighted code...</code></pre>
    </div>
  </div>
</div>
```

The CSS classes are configurable through `CodeHighlightRenderOptions`. When a code block is inside a tabbed group, the standalone container wrapper is omitted.

## Adding a Custom Highlighter

Register an `ICodeHighlighter` with the DI container and Penn picks it up automatically:

```csharp
services.AddSingleton<ICodeHighlighter, MyCustomHighlighter>();
```

Set a priority higher than 50 to beat TextMate for your target languages. The `SupportedLanguages` set controls which languages your highlighter claims. Be specific -- if you only handle `toml`, do not register `"*"` and break everyone else.

Similarly, custom preprocessors are registered as `ICodeBlockPreprocessor` and sorted by priority.

## Why Server-Side?

Penn does highlighting at build time for several reasons:

1. **No FOUC.** Code blocks are already highlighted when the HTML arrives. No flash of unstyled code while a JavaScript library loads.
2. **Smaller payloads.** No need to ship highlight.js or Prism bundles to the client.
3. **Semantic accuracy.** Roslyn highlighting uses the actual C# compiler. It knows that `var` is a keyword but `List` is a type. No regex-based highlighter can match that.
4. **Consistency.** The same highlighting runs during dev and build. What you see in `dotnet watch` is what readers see in production.

The trade-off is build time. Roslyn highlighting, in particular, is not cheap -- it loads your entire solution workspace. But for a documentation site where you build once and serve many times, this is the right trade-off.

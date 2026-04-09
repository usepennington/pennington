---
title: "Showing Code Diffs"
description: "Use the :xmldocid-diff code block modifier with DiffPlex integration to show before/after API comparisons with automatic diff annotations"
uid: "penn.how-to.showing-code-diffs"
order: 20
---

## Beat 1: The `:xmldocid-diff` modifier syntax

Introduce the diff modifier that compares two symbols side-by-side and renders an annotated diff. The code block body contains exactly two XML documentation IDs on separate lines -- the "before" and "after" versions.

### What to show
- Write a fenced code block with the info string `csharp:xmldocid-diff` and two lines in the body: the first is the "old" XML doc ID (`M:Prism.V1.EnumGenerator.Execute(Microsoft.CodeAnalysis.GeneratorExecutionContext)`), the second is the "new" XML doc ID (`M:Prism.V2.EnumGenerator.Execute(Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext)`). Show the rendered output: removed lines styled with `diff-remove`, added lines styled with `diff-add`, unchanged lines rendered normally.
- Explain the strict requirement: `M:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor.ProcessXmlDocIdDiff(System.String,System.String,System.Boolean)` validates that the code block contains exactly 2 XML doc IDs. If the count differs, it emits an error: `"xmldocid-diff requires exactly 2 XmlDocIds, got {count}"` via `T:Pennington.Diagnostics.DiagnosticContext`.
- Show the info string parsing: `M:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor.ParseLanguageId(System.String)` checks for `:xmldocid-diff` before `:xmldocid` to avoid a false prefix match. The base language (e.g., `csharp`) is extracted from the portion before the colon.

### Key points
- The first XML doc ID is the "before" (v1), the second is the "after" (v2) -- order matters for the diff direction
- Each ID is resolved independently via `M:Pennington.Roslyn.Symbols.ISymbolExtractionService.ExtractCodeFragmentAsync(System.String,System.Boolean)` -- both symbols must exist in the workspace
- If either symbol is not found, per-symbol error messages are rendered and a warning is emitted

## Beat 2: Structuring the workspace to contain both versions

For `:xmldocid-diff` to work, both the old and new implementations must be compilable and discoverable in the Roslyn workspace. This beat covers practical patterns for keeping both versions available.

### What to show
- Pattern 1 -- Separate namespaces in separate projects: a `Prism.Legacy` project with `namespace Prism.V1.Generators` and the current `Prism.Generators` project with `namespace Prism.V2.Generators`. Both projects are included in the solution and compile independently.
- Pattern 2 -- Separate namespaces in the same project: use a `Legacy/` folder within the main project. Classes are in a `V1` sub-namespace. This avoids a separate project but requires care to avoid type conflicts.
- Explain how `T:Pennington.Roslyn.Symbols.SymbolExtractionService` discovers symbols: `M:Pennington.Roslyn.Symbols.SymbolExtractionService.ExtractSymbolsAsync(Microsoft.CodeAnalysis.Solution)` iterates all projects (respecting `T:Pennington.Roslyn.ProjectFilter`), gets compilations, and extracts symbols from every document. Both v1 and v2 symbols end up in the same symbol table, keyed by their unique XML doc IDs.
- Mention that `P:Pennington.Roslyn.ProjectFilter.IncludedProjects` must include the legacy project if filtering is enabled.

### Key points
- Both versions must compile without errors for symbol extraction to succeed
- XML doc IDs are inherently unique across namespaces (e.g., `M:Prism.V1.EnumGenerator.Execute(...)` vs `M:Prism.V2.EnumGenerator.Execute(...)`)
- `T:Pennington.Roslyn.Symbols.SymbolExtractionService` uses `ConcurrentDictionary` for thread-safe parallel extraction across projects and documents
- Duplicate symbol IDs are logged at trace level but the first-seen wins

## Beat 3: DiffPlex integration and diff rendering

Deep dive into how the diff is computed and rendered as annotated HTML. Pennington uses the DiffPlex library (declared as a dependency in `Pennington.Roslyn.csproj`) for line-level diff computation.

### What to show
- Walk through `ComputeAndRenderDiff` in `T:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor`: both fragments are first highlighted independently via `T:Pennington.Roslyn.Highlighting.SyntaxHighlighter`, producing highlighted HTML lines. Then `DiffPlex.Differ.CreateLineDiffs` computes the diff on the plain-text versions (with `ignoreWhitespace: true`).
- Explain the rendering loop: for each `DiffBlock`, unchanged lines before the block get `<span class="line">`, deleted lines (from snippet 1) get `<span class="line diff-remove">`, and inserted lines (from snippet 2) get `<span class="line diff-add">`. Remaining unchanged lines after all diff blocks are appended.
- Show the wrapper: when differences exist, the `<pre>` tag gets `class="has-diff"` which enables CSS styling for the diff annotations. The result is a `T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult` with `SkipTransform: true` to prevent further processing.

### Key points
- The diff operates on plain text but renders highlighted HTML -- syntax coloring is preserved within diff-annotated lines
- `ignoreWhitespace: true` means indentation-only changes do not produce false diff lines
- The `has-diff` CSS class on the `<pre>` element allows themes to apply background colors or gutter indicators
- The `T:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor.DiffRenderResult` record carries both the HTML string and a `HasDifferences` boolean

## Beat 4: Use `:xmldocid-diff,bodyonly` for focused implementation diffs

The `,bodyonly` variant strips signatures and braces from both fragments before diffing, focusing the comparison on implementation logic rather than API surface changes.

### What to show
- Change the modifier to `csharp:xmldocid-diff,bodyonly`. Show that the rendered diff now compares only the method bodies -- no `public void Execute(...)` signatures, no opening/closing braces. The diff is cleaner when the signature changed but the reader cares about the logic changes.
- Reference the processing path: `M:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor.TryProcess(System.String,System.String)` maps `"xmldocid-diff,bodyonly"` to `ProcessXmlDocIdDiff(baseLanguage, code, bodyOnly: true)`, which passes `bodyOnly: true` to both `ExtractCodeFragmentAsync` calls.
- Show the body extraction from `T:Pennington.Roslyn.Symbols.CodeFragmentExtractor`: for methods, it returns the block body content or expression body; for types, it returns the content between braces. Both fragments go through `M:Pennington.Roslyn.Utilities.TextFormatter.NormalizeIndents(System.String)` before diffing to align indentation.

### Key points
- `bodyonly` is useful when the method signature changed trivially (e.g., parameter rename) but the implementation changed substantially -- the diff focuses on what matters
- For type-level diffs with `bodyonly`, you see the member list changes without the type declaration line
- The `ParseLanguageId` method detects `,bodyonly` via case-insensitive `Contains` check, so `csharp:xmldocid-diff,BodyOnly` also works

## Beat 5: Show a class-level diff for structural changes

Using type-level XML doc IDs (`T:` prefix) with `:xmldocid-diff` to show structural changes: new fields, removed methods, changed interface implementations, added attributes.

### What to show
- Write a code block comparing `T:Prism.V1.EnumGenerator` with `T:Prism.V2.EnumGenerator`. The rendered diff shows the entire class declarations side-by-side: interface list changes (`ISourceGenerator` to `IIncrementalGenerator`), new fields, removed methods, renamed members.
- Contrast with the `bodyonly` variant: `csharp:xmldocid-diff,bodyonly` on the same type IDs shows only the member bodies without the class declaration and interface list. This is useful when the reader already understands the structural change and wants to focus on internal changes.
- Note that nested types, properties, and fields are all included in the full type extraction via `SyntaxNode.ToFullString()` in `M:Pennington.Roslyn.Symbols.CodeFragmentExtractor.ExtractCodeFragmentAsync(Microsoft.CodeAnalysis.SyntaxNode,System.String,System.Boolean)`.

### Key points
- Type-level diffs are ideal for migration guides that need to show "here is everything that changed"
- Method-level diffs are better for focused "here is how this specific API changed" explanations
- Combine both in a migration guide: start with the class-level diff for overview, then drill into individual method diffs

## Beat 6: Error handling and diagnostics

What happens when symbols are not found, when the workspace fails to load, or when the code block is malformed.

### What to show
- Show the three error cases in `ProcessXmlDocIdDiff`: wrong number of XML doc IDs (not exactly 2), symbol not found for one or both IDs, and exceptions during processing. Each case renders an HTML error message and logs via `T:Pennington.Diagnostics.DiagnosticContext` (using `AddWarning` for missing symbols, `AddError` for structural problems).
- Show how errors appear in development: when running with `dotnet watch`, the diagnostic overlay (injected by `T:Pennington.Infrastructure.DiagnosticOverlayProcessor`) surfaces warnings about unresolved symbols. Reference `M:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor.ProcessXmlDocIdDiff(System.String,System.String,System.Boolean)` for the error rendering pattern.
- Mention that `SkipTransform: true` is set on error results to prevent downstream code transformers from attempting to process the error HTML.

### Key points
- Errors are visible during development but degrade gracefully in production -- the page still renders, just with an error comment in the code block
- The diagnostic overlay shows all `xmldocid-diff` resolution warnings per request
- A common cause of "symbol not found" is a missing project in `T:Pennington.Roslyn.ProjectFilter` -- verify the legacy project is included

## Beat 7: Best practices for diff documentation

Practical guidance on maintaining diff-based documentation that stays useful and accurate.

### What to show
- Recommend keeping legacy code in a dedicated project (e.g., `Prism.Legacy`) or namespace rather than using `#if` directives. Both versions must compile, and `#if` can make the code harder to maintain.
- Advise adding XML doc comments to both the old and new versions explaining the rationale for the change. Even though the diff modifier does not render XML docs, the documentation metadata in `P:Pennington.Roslyn.Symbols.SymbolInfo.XmlDocumentation` can be consumed by future features.
- Show a complete migration guide structure: introductory text explaining the motivation, a class-level diff for the big picture, focused method-level diffs for each changed API, and interleaved explanatory text guiding the reader through each change.
- Both versions must be in projects included in the solution. Note: `ProjectFilter` currently does not affect symbol extraction — all projects in the solution are scanned for symbols regardless of the filter setting.

### Key points
- Diff documentation is most valuable for migration guides, changelog pages, and "what's new" sections
- Keep the legacy code compilable -- broken code cannot be extracted by Roslyn
- Use interleaved markdown text between diff code blocks to narrate the migration steps
- The `T:Pennington.Roslyn.Symbols.SymbolExtractionService` caches the symbol table lazily via `AsyncLazy`. Note: `M:Pennington.Roslyn.Symbols.ISymbolExtractionService.ClearCache` exists but is not currently called by `InvalidateSolution` — after structural file changes, the symbol cache may be stale. This is a known gap

---
title: "Connecting to a Roslyn Workspace"
description: "Add Pennington.Roslyn, configure RoslynOptions with SolutionPath and ProjectFilter, and use :xmldocid, :xmldocid,bodyonly, and :path code block modifiers to embed live code from your .NET solution"
uid: "penn.how-to.connecting-to-a-roslyn-workspace"
order: 10
---

## Beat 1: Install Pennington.Roslyn and register the Roslyn services

The Pennington.Roslyn package provides Roslyn-powered code highlighting and the ability to embed live source code from a .NET solution into documentation pages. This beat covers adding the NuGet package and calling the registration extension method.

### What to show
- Install the `Pennington.Roslyn` NuGet package into the docs project. Show the package reference in the `.csproj` (note: the package targets `net11.0;net10.0` and depends on `Microsoft.CodeAnalysis.CSharp.Workspaces`, `Microsoft.CodeAnalysis.Workspaces.MSBuild`, `Microsoft.Build.Locator`, and `DiffPlex`). Reference `:path src/Pennington.Roslyn/Pennington.Roslyn.csproj`.
- In `Program.cs`, call `M:Pennington.Roslyn.RoslynExtensions.AddPenningtonRoslyn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Roslyn.RoslynOptions})` after `AddDocSite`. Show the minimal registration: `services.AddPenningtonRoslyn(roslyn => { roslyn.SolutionPath = "../../Prism.slnx"; });`.
- Explain that `AddPenningtonRoslyn` always registers `T:Pennington.Roslyn.Highlighting.SyntaxHighlighter` (AdhocWorkspace-based Roslyn Classifier API highlighter) and `T:Pennington.Roslyn.Highlighting.RoslynHighlighter` (which implements `T:Pennington.Highlighting.ICodeHighlighter` at priority 100, beating TextMate at priority 50). When `SolutionPath` is set, it additionally registers `T:Pennington.Roslyn.Workspace.SolutionWorkspaceService`, `T:Pennington.Roslyn.Symbols.SymbolExtractionService`, and `T:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor`.

### Key points
- `Pennington.Roslyn` is a separate package so sites that do not need Roslyn analysis avoid the heavy MSBuild/Roslyn dependency tree
- Without `SolutionPath`, only Roslyn-powered syntax highlighting is enabled (no `:xmldocid` or `:path` modifiers)
- `SolutionPath` accepts both `.sln` and `.slnx` files

## Beat 2: Configure RoslynOptions with SolutionPath and ProjectFilter

Detailed walkthrough of the two configuration properties on `T:Pennington.Roslyn.RoslynOptions`: pointing at the solution file and optionally filtering which projects are analyzed.

### What to show
- Show the full `T:Pennington.Roslyn.RoslynOptions` class: `P:Pennington.Roslyn.RoslynOptions.SolutionPath` (nullable string, path to `.sln` or `.slnx`) and `P:Pennington.Roslyn.RoslynOptions.ProjectFilter` (nullable `T:Pennington.Roslyn.ProjectFilter`).
- Show the `T:Pennington.Roslyn.ProjectFilter` record with its two properties: `P:Pennington.Roslyn.ProjectFilter.IncludedProjects` (`HashSet<string>?`) and `P:Pennington.Roslyn.ProjectFilter.ExcludedProjects` (`HashSet<string>?`). Demonstrate filtering to only Prism projects: `roslyn.ProjectFilter = new ProjectFilter { IncludedProjects = new HashSet<string> { "Prism.Generators", "Prism.Core" } };`.
- Explain how `T:Pennington.Roslyn.Workspace.SolutionWorkspaceService` uses the filter in `M:Pennington.Roslyn.Workspace.SolutionWorkspaceService.GetProjectsAsync(System.Func{Microsoft.CodeAnalysis.Project,System.Boolean})` via the private `ApplyProjectFilter` method: included projects are whitelisted, excluded projects are blacklisted, and both can be combined.

### Key points
- `SolutionPath` is relative to the docs project's working directory (the folder containing the docs `.csproj`)
- Filtering projects dramatically reduces initial load time because `SolutionWorkspaceService` only compiles the included projects
- If neither `IncludedProjects` nor `ExcludedProjects` is set, all projects in the solution are analyzed

## Beat 3: Use the `:xmldocid` modifier to embed a full symbol declaration

The `:xmldocid` modifier on a fenced code block replaces the code block's content with the live source code of the specified symbol, looked up by XML documentation ID format.

### What to show
- Write a fenced code block in a markdown page with the info string `csharp:xmldocid` and the body containing the XML documentation ID, e.g., `T:Prism.Generators.EnumGenerator`. Show the rendered result: the full class declaration extracted from the actual source file.
- Explain the XML documentation ID format: `T:` for types, `M:` for methods (with parameter types), `P:` for properties, `F:` for fields, `E:` for events. Method parameters use fully qualified type names: `M:Prism.Generators.EnumGenerator.Execute(Microsoft.CodeAnalysis.GeneratorExecutionContext)`.
- Walk through the processing pipeline: `M:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor.TryProcess(System.String,System.String)` calls `ParseLanguageId` to extract the modifier, then `ProcessXmlDocId` which splits the code block content into XML doc IDs (one per line), calls `M:Pennington.Roslyn.Symbols.ISymbolExtractionService.ExtractCodeFragmentAsync(System.String,System.Boolean)` with `bodyOnly: false`, and highlights each fragment using `T:Pennington.Roslyn.Highlighting.SyntaxHighlighter`.
- Show that multiple XML doc IDs can be placed on separate lines in a single code block -- each is resolved and concatenated with blank lines between them.

### Key points
- The `T:Pennington.Roslyn.Symbols.XmlDocIdNormalizer` strips namespace prefixes from parameter types so that both `M:Type.Method(System.String)` and `M:Type.Method(String)` resolve to the same symbol
- If a symbol is not found, a warning is emitted via `T:Pennington.Diagnostics.DiagnosticContext` and an error comment is rendered in the code block
- The extracted code is fed through `M:Pennington.Roslyn.Utilities.TextFormatter.NormalizeIndents(System.String)` to strip common leading whitespace

## Beat 4: Use the `:xmldocid,bodyonly` modifier for method bodies without ceremony

The `,bodyonly` variant extracts only the implementation body of a method, property, or type -- stripping the signature, braces, and outer declaration.

### What to show
- Change the code block modifier to `csharp:xmldocid,bodyonly` with a method's XML doc ID. Show that only the method body is rendered (no `public void Execute(...)` signature, no opening/closing braces).
- Explain when this is useful: tutorials that want to focus on logic rather than ceremony, step-by-step walkthroughs of an algorithm, or showing just the expression body of a property.
- Reference the extraction logic in `T:Pennington.Roslyn.Symbols.CodeFragmentExtractor`: `M:Pennington.Roslyn.Symbols.CodeFragmentExtractor.ExtractCodeFragmentAsync(Microsoft.CodeAnalysis.SyntaxNode,System.String,System.Boolean)` delegates to the private `ExtractBody` method which pattern-matches on the syntax node type -- `MethodDeclarationSyntax` returns the expression body or block body content, `PropertyDeclarationSyntax` returns the expression body or accessor list, `TypeDeclarationSyntax` returns the content between braces.

### Key points
- For methods with expression bodies (`=>`), `bodyonly` returns just the expression
- For methods with block bodies, `bodyonly` returns everything between the braces, trimmed
- For types (classes, records, enums), `bodyonly` returns the member declarations without the type's own signature
- `bodyonly` is passed through `M:Pennington.Roslyn.Symbols.SymbolExtractionService.ExtractCodeFragmentAsync(System.String,System.Boolean)` as the second parameter

## Beat 5: Use the `:path` modifier to include entire files by path

The `:path` modifier includes an entire file's content from the solution directory, useful for `.csproj` files, configuration files, or any non-C# source that does not have XML doc IDs.

### What to show
- Write a code block with info string `xml:path` and body containing a relative path like `samples/Prism.Sample/Prism.Sample.csproj`. Show the rendered result: the complete file content, syntax-highlighted.
- Walk through the processing in the `ProcessPath` method of `T:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor`: it validates the path (rejects `..` and rooted paths to prevent directory traversal), resolves the path relative to the solution directory (`Path.GetDirectoryName` of `P:Pennington.Roslyn.RoslynOptions.SolutionPath`), reads the file, and highlights it via `T:Pennington.Roslyn.Highlighting.SyntaxHighlighter`.
- Show that the base language from the info string (e.g., `xml`, `json`, `csharp`) controls CSS class assignment but the highlighter detects language from the file extension when available.

### Key points
- Paths are relative to the solution root directory, not the docs project
- The path validator blocks directory traversal (`..`) and absolute paths for security
- If `SolutionPath` is not configured, `:path` blocks emit an error comment
- The language for highlighting is detected from the file extension (`.vb` maps to VisualBasic, everything else defaults to CSharp for Roslyn highlighting)

## Beat 6: Live reload with dotnet watch

When the Roslyn workspace is connected and the docs site runs under `dotnet watch`, editing a `.cs` file in the solution triggers an automatic workspace update and page re-render.

### What to show
- Run `dotnet watch run --project docs/Prism.Docs`. Edit the `Execute` method in `EnumGenerator.cs`. Save and observe the browser refreshing with the updated code block content.
- Explain the file watching integration: `T:Pennington.Roslyn.Workspace.SolutionWorkspaceService` subscribes to `T:Pennington.Infrastructure.IFileWatcher` in its constructor. It registers watchers for `*.cs`, `*.csproj`, `*.sln`, and `*.slnx` files within the solution directory.
- Detail the smart update strategy: when a `.cs` file's content changes (`WatcherChangeTypes.Changed`), `M:Pennington.Roslyn.Workspace.ISolutionWorkspaceService.UpdateDocument(System.String)` enqueues a deferred update. When a structural change occurs (file created, deleted, or renamed), `M:Pennington.Roslyn.Workspace.ISolutionWorkspaceService.InvalidateSolution` forces a full solution reload. Deferred updates are applied in batch on the next `M:Pennington.Roslyn.Workspace.ISolutionWorkspaceService.LoadSolutionAsync(System.String)` call.
- Mention that `T:Pennington.Roslyn.Symbols.SymbolExtractionService` uses `AsyncLazy` for lazy symbol table loading. Note: `M:Pennington.Roslyn.Symbols.ISymbolExtractionService.ClearCache` exists but is not currently called by `InvalidateSolution` — the symbol cache may become stale after structural changes until a full reload occurs. This is a known gap.

### Key points
- Content changes (editing a `.cs` file) use incremental document updates via `Roslyn.Solution.WithDocumentText` -- fast, no full reload
- Structural changes (adding/removing files, editing `.csproj`) trigger full solution invalidation and reload
- The `SolutionWorkspaceService` creates a temporary build directory to avoid polluting real build output
- Compilation results are cached per-project and invalidated only when that project's documents change

## Beat 7: Best practices for documenting live code

Guidance on structuring a project so that Roslyn-backed documentation stays maintainable and accurate.

### What to show
- Recommend pointing `SolutionPath` at the same solution the team develops against -- the documentation always reflects the latest code without manual updates.
- Advise using XML doc comments (`///`) on the source code itself so that `P:Pennington.Roslyn.Symbols.SymbolInfo.XmlDocumentation` is populated. This helps when tooling or future features consume the documentation metadata.
- Show a pattern for organizing code blocks: use `:xmldocid` for full type overviews, `:xmldocid,bodyonly` for focused explanations of specific methods, and `:path` for non-code configuration files.
- Warn about workspace load time: a large solution with many projects benefits from `T:Pennington.Roslyn.ProjectFilter` to scope analysis. The `P:Pennington.Roslyn.ProjectFilter.ExcludedProjects` property is useful for skipping test projects. Note: `ProjectFilter` currently affects which projects are returned by `GetProjectsAsync` but does not limit symbol extraction — `ExtractSymbolsAsync` processes all projects in the solution. This is a known gap.

### Key points
- Keep documentation code blocks referencing real symbols -- avoid copying code into markdown that will drift out of sync
- `ProjectFilter` currently affects project enumeration but not symbol extraction — all symbols from all projects are extracted regardless of the filter. The filter still reduces compilation caching overhead for non-documentation projects
- The `T:Pennington.Roslyn.Symbols.SymbolInfo` record captures the full context: `Symbol`, `Document`, `SyntaxNode`, `SourceText`, `TextSpan`, `XmlDocumentation`, and `Project`
- `T:Pennington.Roslyn.Utilities.TextFormatter` normalizes indentation so extracted fragments render cleanly regardless of their nesting depth in the original source file

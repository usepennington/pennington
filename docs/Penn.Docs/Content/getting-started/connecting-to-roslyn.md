---
title: "Connecting to Roslyn"
description: "Install and configure Penn.Roslyn to pull live code from your .NET solution into documentation"
uid: "penn.getting-started.connecting-to-roslyn"
order: 1020
---

Penn.Roslyn connects your documentation to your actual source code. Instead of copying code samples into markdown files, you reference symbols by their XML documentation ID or include files by path. When your code changes, your documentation updates automatically.

Penn.Roslyn is a separate package. Penn core handles markdown and content rendering; Penn.Roslyn adds the ability to reach into a .NET solution and extract code. You opt in only if you need it.

## What You'll Learn

- Install and configure `Penn.Roslyn` as a separate package
- Point `RoslynOptions` at your .NET solution
- Filter which projects get analyzed
- Reference code symbols with `:xmldocid` blocks
- Include entire source files with `:path` blocks
- Show API differences with `:xmldocid-diff` blocks
- Set up file watching for `.cs` files

## Prerequisites

- Completed the [Creating Your First Site](xref:penn.getting-started.creating-first-site) tutorial
- A .NET solution (`.sln` or `.slnx`) with code you want to document
- Familiarity with [XML documentation ID format](xref:penn.reference.xmldocid-format) (the `T:`, `M:`, `P:`, `F:` prefixes)

<Steps>
<Step stepNumber="1">
## Install Penn.Roslyn

Add the Penn.Roslyn NuGet package to your doc site project:

```bash
dotnet add package Penn.Roslyn --prerelease
```

This is separate from `Penn.DocSite`. Roslyn adds a meaningful dependency footprint, so it ships as its own package.
</Step>

<Step stepNumber="2">
## Configure Roslyn in Program.cs

Register Penn.Roslyn services by calling `AddPennRoslyn` on your service collection. This is a separate call from `AddDocSite`:

```csharp
using Penn.DocSite;
using Penn.Roslyn;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "A site about things",
    SolutionPath = "../../MySolution.slnx",
});

builder.Services.AddPennRoslyn(roslyn =>
{
    roslyn.SolutionPath = "../../MySolution.slnx";
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

`SolutionPath` points to your `.sln` or `.slnx` file, relative to your doc site project root. Penn.Roslyn loads the solution via the Roslyn workspace API, giving it access to every type, method, and property in your codebase.

Here is the `RoslynOptions` type:

```csharp:xmldocid
T:Penn.Roslyn.RoslynOptions
```

When `SolutionPath` is null, Penn.Roslyn still registers a Roslyn-based syntax highlighter but skips symbol extraction and file inclusion. Set `SolutionPath` to enable the full feature set.

> [!NOTE]
> If you call `AddPennRoslyn` without configuring `SolutionPath`, you get improved syntax highlighting but none of the `:xmldocid` or `:path` code block features.
</Step>

<Step stepNumber="3">
## Filter Projects with ProjectFilter (Optional)

If your solution contains many projects and you only document a few, use `ProjectFilter` to narrow the scope. This reduces load time and memory usage:

```csharp
builder.Services.AddPennRoslyn(roslyn =>
{
    roslyn.SolutionPath = "../../MySolution.slnx";
    roslyn.ProjectFilter = new ProjectFilter
    {
        IncludedProjects = ["MyApp.Core", "MyApp.Models"],
    };
});
```

You can also exclude specific projects instead:

```csharp
builder.Services.AddPennRoslyn(roslyn =>
{
    roslyn.SolutionPath = "../../MySolution.slnx";
    roslyn.ProjectFilter = new ProjectFilter
    {
        ExcludedProjects = ["MyApp.Tests", "MyApp.Benchmarks"],
    };
});
```

> [!TIP]
> For solutions with more than a dozen projects, filtering noticeably speeds up startup. Only include the projects that contain symbols you reference in your documentation.
</Step>

<Step stepNumber="4">
## Use `:xmldocid` to Reference Code Symbols

Instead of pasting code into your markdown, reference it by XML documentation ID. Penn.Roslyn finds the symbol in your solution and renders the current source code with syntax highlighting.

Write a fenced code block with `:xmldocid` appended to the language identifier:

``````markdown
```csharp:xmldocid
T:Penn.Infrastructure.PennOptions
```
``````

This renders the full source of `PennOptions` as it exists in your codebase. When the class changes, your documentation reflects those changes automatically.

### Symbol Format Reference

XML documentation IDs follow .NET conventions:

| Prefix | Targets | Example |
|--------|---------|---------|
| `T:` | Types (classes, records, interfaces, enums) | `T:Penn.FrontMatter.DocFrontMatter` |
| `M:` | Methods | `M:Penn.Roslyn.RoslynExtensions.AddPennRoslyn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Roslyn.RoslynOptions})` |
| `P:` | Properties | `P:Penn.Roslyn.RoslynOptions.SolutionPath` |
| `F:` | Fields | `F:MyApp.Constants.MaxRetries` |

### Body-Only Mode

To show only the implementation without the declaration, append `,bodyonly` to the modifier:

``````markdown
```csharp:xmldocid,bodyonly
M:MyApp.Services.DataService.ProcessAsync
```
``````

This strips the method signature and braces, showing only the body. Use this when walking through implementation details.

### Multiple Symbols in One Block

Reference multiple symbols in a single code block by placing each on its own line:

``````markdown
```csharp:xmldocid
T:Penn.FrontMatter.IDraftable
T:Penn.FrontMatter.ITaggable
T:Penn.FrontMatter.IOrderable
```
``````

Penn.Roslyn extracts each symbol and renders them together, separated visually. This works well for showing a family of related types.
</Step>

<Step stepNumber="5">
## Use `:path` to Include Entire Files

The `:path` modifier includes a file from your solution by its path relative to the solution root:

``````markdown
```csharp:path
docs/Penn.Docs/Program.cs
```
``````

This reads the full contents of the file from disk and renders it with syntax highlighting. It works for any file type Penn can highlight:

``````markdown
```yaml:path
examples/MinimalExample/Content/index.md
```
``````

The path is always relative to the solution directory (wherever your `.slnx` or `.sln` lives). Directory traversal with `..` is not allowed. Rooted paths are also rejected.

> [!WARNING]
> The `:path` modifier requires `SolutionPath` to be configured in `RoslynOptions`. Without it, Penn.Roslyn cannot resolve the solution directory.
</Step>

<Step stepNumber="6">
## Use `:xmldocid-diff` to Show API Evolution

The `:xmldocid-diff` modifier takes exactly two XML documentation IDs and renders a line-level diff between them:

``````markdown
```csharp:xmldocid-diff
T:Penn.FrontMatter.IFrontMatter
T:Penn.FrontMatter.DocFrontMatter
```
``````

Penn.Roslyn extracts both symbols, computes line differences, and renders the result with added and removed line indicators. Lines unique to the first symbol appear as removals. Lines unique to the second appear as additions.

You can also compare just the bodies with `,bodyonly`:

``````markdown
```csharp:xmldocid-diff,bodyonly
M:MyApp.Services.OldService.Process
M:MyApp.Services.NewService.Process
```
``````

This is effective for migration guides and changelog entries where you want to show exactly what changed between two implementations.
</Step>

<Step stepNumber="7">
## Set Up File Watching for .cs Files

Since your documentation now depends on source code, configure `dotnet watch` to pick up changes to both content files and C# files. Update the `<Watch>` items in your doc site `.csproj`:

```xml
<ItemGroup>
    <Watch Include="Content\**\*.*" />
    <Watch Include="..\..\src\**\*.cs" />
</ItemGroup>
```

Adjust the source path to match where your code lives relative to your doc site project. Keep the path specific to source directories to avoid watching build artifacts.
</Step>

<Step stepNumber="8">
## Putting It All Together

Create a content page that exercises all three modifiers. Here is an example `Content/api-overview.md`:

```markdown
---
title: "API Overview"
description: "Penn's core configuration types"
order: 10
---

## RoslynOptions

The configuration object for Roslyn integration:

 ```csharp:xmldocid
T:Penn.Roslyn.RoslynOptions
 ```

## Full Program.cs

A complete working configuration:

 ```csharp:path
docs/Penn.Docs/Program.cs
 ```

## Interface vs Implementation

The minimal front matter interface compared to the full doc site record:

 ```csharp:xmldocid-diff
T:Penn.FrontMatter.IFrontMatter
T:Penn.DocSite.DocSiteFrontMatter
 ```
```

Run your site with `dotnet watch` and visit the page. Every code block is live, sourced from your codebase, highlighted by Roslyn, and updated whenever you change the underlying code.
</Step>
</Steps>

## How It Works

Penn.Roslyn registers an `ICodeBlockPreprocessor` that intercepts fenced code blocks during markdown rendering:

1. It parses the language modifier (`:xmldocid`, `:path`, or `:xmldocid-diff`) from the code fence
2. For `:xmldocid`, it resolves the XML documentation ID against the loaded Roslyn workspace, extracts the syntax node, and highlights it with Roslyn's syntax API
3. For `:path`, it reads the file from disk relative to the solution root and highlights it
4. For `:xmldocid-diff`, it extracts both symbols and runs a line-level diff algorithm (via DiffPlex), rendering additions and removals with CSS classes

All processing happens at render time. In dev mode, source file changes trigger a re-render. In build mode, the state is captured at build time.

## Best Practices

- **Reference real code, not contrived examples.** Keep example code in your solution (an `examples/` project works well) and reference it with `:xmldocid` or `:path`.
- **Use `ProjectFilter` for large solutions.** Roslyn loads every project by default. Filtering to documented projects reduces startup time.
- **Prefer `:xmldocid` over `:path` for types and methods.** Symbol-level extraction is more precise and survives file reorganization. Use `:path` for complete files like `Program.cs`.
- **Use `,bodyonly` for implementation walkthroughs.** Strip the declaration boilerplate when the focus is on what the code does, not its signature.
- **Handle unresolved symbols gracefully.** If a symbol ID is wrong, Penn.Roslyn renders an error comment in the code block and logs a diagnostic warning. Check your dev console for these.

## Next Steps

- [Using UI Elements](xref:penn.getting-started.using-ui-elements) -- enhance your pages with cards, badges, and step-by-step layouts
- [Deploying to GitHub Pages](xref:penn.getting-started.deploying-to-github-pages) -- ship your site with GitHub Actions

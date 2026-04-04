---
title: "Connecting to Roslyn"
description: "Add Penn.Roslyn for live code extraction and syntax highlighting from your .NET solution"
uid: "penn.getting-started.connecting-to-roslyn"
order: 1020
---

Here's the problem with code samples in documentation: they lie. You write them once, the API evolves, and now your docs show code that doesn't compile. Nobody notices until a user copies it, pastes it, and files an issue titled "your example doesn't work."

Penn.Roslyn fixes this by pulling code directly from your actual source files and compiled symbols. Your documentation references your real code, and when that code changes, the docs change with it. No copying, no pasting, no lying.

In v2, Roslyn support is a **separate optional package**. Penn core handles markdown and content; Penn.Roslyn adds the ability to reach into your .NET solution and extract code by XML documentation ID or file path. You opt in only if you need it.

## What You'll Learn

- Install and configure `Penn.Roslyn` as a separate package
- Reference code symbols with `:xmldocid` blocks
- Include entire source files with `:path` blocks
- Show API evolution with `:xmldocid-diff` blocks

## Prerequisites

- Completed the [Creating Your First Site](xref:penn.getting-started.creating-first-site) tutorial
- A .NET solution with some code worth documenting
- Familiarity with [XML documentation ID format](xref:penn.reference.xmldocid-format) (the `T:`, `M:`, `P:` prefixes)

<Steps>
<Step stepNumber="1">
## Install Penn.Roslyn

Penn.Roslyn is its own NuGet package. This is deliberate — Roslyn adds a meaningful dependency footprint, and not every site needs live code extraction. Add it to your doc site project:

```bash
dotnet add package Penn.Roslyn --prerelease
```
</Step>

<Step stepNumber="2">
## Configure Roslyn Integration

Update your `Program.cs` to register Penn.Roslyn services. This is a separate call from `AddDocSite` — it chains onto `builder.Services` directly:

```csharp
using Penn.DocSite;
using Penn.Roslyn;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(_ => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "A site about things",
    SolutionPath = "../../MySolution.slnx",
});

// Roslyn integration — separate package, separate registration
builder.Services.AddPennRoslyn(roslyn =>
{
    roslyn.SolutionPath = "../../MySolution.slnx";
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

The `SolutionPath` points to your `.sln` or `.slnx` file, relative to your doc site project root. Penn.Roslyn loads the solution via the Roslyn workspace API, which gives it access to every type, method, and property in your codebase.

Here's what `RoslynOptions` looks like:

```csharp:xmldocid
T:Penn.Roslyn.RoslynOptions
```

The `ProjectFilter` lets you narrow which projects get analyzed — useful if your solution has dozens of projects and you only document a few.
</Step>

<Step stepNumber="3">
## Watch Source Code Changes

Since your documentation now depends on your source code, you'll want `dotnet watch` to pick up changes in both. Update the `<Watch>` items in your `.csproj`:

```xml
<ItemGroup>
    <Watch Include="Content\**\*.*" />
    <Watch Include="..\..\src\**\*.cs" />
</ItemGroup>
```

The source path is relative to your project root, not the solution root. Adjust it to match where your source code lives relative to your doc site project. Exclude build artifacts by keeping the path specific to source directories.
</Step>

<Step stepNumber="4">
## Use `:xmldocid` to Reference Code Symbols

This is where it gets good. Instead of pasting code into your markdown, you reference it by its XML documentation ID. Penn.Roslyn finds the symbol in your solution and renders the current source code.

In your markdown, write a fenced code block with `:xmldocid` appended to the language:

``````markdown
```csharp:xmldocid
T:Penn.Infrastructure.PennOptions
```
``````

This renders the full source of the `PennOptions` class, with Roslyn-powered syntax highlighting. When `PennOptions` changes — new properties, renamed members, updated doc comments — your documentation reflects it automatically.

### Symbol Reference Formats

XML documentation IDs follow .NET conventions:

| Prefix | Targets | Example |
|--------|---------|---------|
| `T:` | Types (classes, records, interfaces, enums) | `T:Penn.FrontMatter.DocFrontMatter` |
| `M:` | Methods | `M:Penn.Infrastructure.PennOptions.AddMarkdownContent``1(System.Action{Penn.Infrastructure.MarkdownContentOptions})` |
| `P:` | Properties | `P:Penn.Infrastructure.PennOptions.SiteTitle` |
| `F:` | Fields | `F:MyApp.Constants.MaxRetries` |

### Body-Only Mode

Sometimes you want just the implementation without the declaration boilerplate. Append `,bodyonly` to the modifier:

``````markdown
```csharp:xmldocid,bodyonly
M:MyApp.Services.DataService.ProcessAsync
```
``````

This strips the method signature and braces, showing only the body. Useful for walking through implementation details in a tutorial.

### Multiple Symbols in One Block

You can reference multiple symbols in a single code block — each on its own line:

``````markdown
```csharp:xmldocid
T:Penn.FrontMatter.IDraftable
T:Penn.FrontMatter.ITaggable
T:Penn.FrontMatter.IOrderable
```
``````

Penn.Roslyn extracts each symbol and renders them together, separated visually in the output. Handy for showing a family of related types.
</Step>

<Step stepNumber="5">
## Use `:path` to Include Entire Files

Sometimes you want to show a complete file rather than a single symbol. The `:path` modifier includes a file from your solution by its path relative to the solution root:

``````markdown
```csharp:path
docs/Penn.Docs/Program.cs
```
``````

This pulls in the full contents of `Program.cs` as it exists on disk, with syntax highlighting. It works for any file type that Penn can highlight — `.cs`, `.razor`, `.json`, `.xml`, `.yaml`, and more:

``````markdown
```yaml:path
examples/MinimalExample/Content/index.md
```
``````

The path is always relative to the solution directory (wherever your `.slnx` or `.sln` lives). No `..` traversal is allowed — Penn.Roslyn validates paths to prevent shenanigans.
</Step>

<Step stepNumber="6">
## Use `:xmldocid-diff` to Show API Evolution

This one's genuinely useful for changelogs and migration guides. The `:xmldocid-diff` modifier takes two XML documentation IDs and renders a diff between them:

``````markdown
```csharp:xmldocid-diff
T:Penn.FrontMatter.IFrontMatter
T:Penn.FrontMatter.DocFrontMatter
```
``````

Penn.Roslyn extracts both symbols, computes a line-level diff, and renders the result with added/removed line indicators. Lines unique to the first symbol appear as removals; lines unique to the second appear as additions. Shared lines render normally.

Like `:xmldocid`, you can append `,bodyonly` to compare just the bodies:

``````markdown
```csharp:xmldocid-diff,bodyonly
M:MyApp.Services.OldService.Process
M:MyApp.Services.NewService.Process
```
``````

This is particularly effective for "here's what changed" sections in upgrade guides.
</Step>

<Step stepNumber="7">
## Putting It All Together

Create a content page that exercises all three modifiers. Here's `Content/api-reference.md`:

```markdown
---
title: "API Reference"
description: "Penn's core configuration types"
order: 10
---

## PennOptions

The main configuration object for the Penn engine:

 ```csharp:xmldocid
T:Penn.Infrastructure.PennOptions
 ```

## Full Program.cs

Here's a complete working configuration:

 ```csharp:path
docs/Penn.Docs/Program.cs
 ```

## Front Matter: Interface vs Implementation

The minimal interface compared to the full doc site record:

 ```csharp:xmldocid-diff
T:Penn.FrontMatter.IFrontMatter
T:Penn.DocSite.DocSiteFrontMatter
 ```
```

Run your site and visit the page. Every code block is live — sourced from your actual codebase, highlighted by Roslyn, and updated whenever you change the underlying code.
</Step>
</Steps>

## How It Works

When Penn.Roslyn processes your markdown:

1. It parses the language modifier (`:xmldocid`, `:path`, or `:xmldocid-diff`)
2. For `:xmldocid`, it resolves the XML documentation ID against the loaded Roslyn workspace, extracts the syntax node, and highlights it
3. For `:path`, it reads the file from disk relative to the solution root and highlights it
4. For `:xmldocid-diff`, it extracts both symbols and runs a line-level diff algorithm

All of this happens at render time. In dev mode, changes to source files trigger a re-render. In build mode, it captures the state at build time.

## Best Practices

- **Reference real code, not contrived examples.** The whole point is that your docs stay honest. If your examples live in an `examples/` project in your solution, reference those.
- **Use `ProjectFilter` for large solutions.** Roslyn loads every project by default. If your solution has 40 projects and you only document 3, filtering speeds things up noticeably.
- **Prefer `:xmldocid` over `:path` for types and methods.** Symbol-level extraction is more precise and survives file reorganization. Use `:path` for complete files like `Program.cs` where the whole file is the example.

## Next Steps

- [Using UI Elements](xref:penn.getting-started.using-ui-elements) — enhance your pages with cards, badges, and step-by-step layouts
- [Deploying to GitHub Pages](xref:penn.getting-started.deploying-to-github-pages) — ship your site with GitHub Actions

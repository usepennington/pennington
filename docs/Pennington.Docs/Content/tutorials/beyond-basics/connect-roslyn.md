---
title: "Connect to a Roslyn solution for live API snippets"
description: "Point Pennington at a .sln, pull method and class source into markdown with xmldocid fences, and watch hot reload refresh snippets when the source changes."
sectionLabel: Beyond the Basics
order: 104020
tags: [roslyn, api-docs, xmldocid, hot-reload]
uid: tutorials.beyond-basics.connect-roslyn
---

By the end of this tutorial, your DocSite host loads a sibling C# class library through an inner `.slnx` and renders live `Calculator` and `Greeter` source inside a markdown page via `csharp:xmldocid` fences. You'll see how to register `Pennington.Roslyn`, set `SolutionPath`, write the three fence variants (`:xmldocid`, `:xmldocid,bodyonly`, and multi-symbol), and confirm that hot reload refreshes snippets when the backing source changes.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) (or have an equivalent DocSite host ready)
- A C# project or class library to fence into docs (we'll build a tiny one in unit 1)

The finished code for this tutorial lives in [`examples/BeyondRoslynExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondRoslynExample).

---

## 1. Give your host a sibling library to fence

Before `Pennington.Roslyn` can pull source, there needs to be a `.slnx` listing the project that holds the types to embed. This unit stands up the dual-project shape the rest of the tutorial uses.

<Steps>
<Step StepNumber="1">

**Review the starting DocSite host**

This is the plain DocSite from the scaffold tutorial with no Roslyn wired yet. A `csharp:xmldocid` fence dropped into a markdown page right now renders as a literal code block, because no preprocessor is registered.

```csharp:xmldocid,bodyonly,usings
M:BeyondRoslynExample.Stage1.Run(System.String[])
```

</Step>
<Step StepNumber="2">

**Add a sibling `Sample` class library**

Drop a `Sample/BeyondRoslynExample.Sample.csproj` folder next to the host csproj. Set `GenerateDocumentationFile=true` so XmlDocId lookups resolve. Also set `DefaultItemExcludes` on the host csproj to skip `Sample\**` — otherwise the two projects compete over the same `.cs` files.

</Step>
<Step StepNumber="3">

**Add two small types to fence**

These are the symbols the rest of the tutorial points at. The XML doc comments on each member are what make them addressable by XmlDocId.

```csharp:xmldocid
T:BeyondRoslynExample.Sample.Calculator
```

```csharp:xmldocid
T:BeyondRoslynExample.Sample.Greeter
```

</Step>
<Step StepNumber="4">

**Write an inner `BeyondRoslynExample.slnx`**

Create an inner `.slnx` that registers only the Sample library. `SolutionPath` points at this file rather than the outer repo-level solution, so the MSBuild workspace loads exactly the source to fence into docs.

```text:path
examples/BeyondRoslynExample/BeyondRoslynExample.slnx
```

> [!NOTE]
> On the .NET 11 preview SDK, `dotnet new sln` emits an XML `.slnx` by default. If you prefer the legacy `.sln` format, pass `--format sln`. `SolutionPath` accepts either extension — `Pennington.Roslyn` uses `Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace`, which opens both.

</Step>
</Steps>

### Checkpoint — Two projects, one inner slnx

- Run `dotnet build` on both csprojs — they compile independently
- `BeyondRoslynExample.slnx` lives next to the host csproj and lists only `Sample/BeyondRoslynExample.Sample.csproj`
- Run `dotnet run` on the host — DocSite still serves, nothing has changed in the browser yet

---

## 2. Register `Pennington.Roslyn` and set `SolutionPath`

A single DI call turns on the xmldocid preprocessor. Once `AddPenningtonRoslyn` runs with `SolutionPath` set, every markdown page in the content folder gains the `:xmldocid`, `:xmldocid,bodyonly`, `:xmldocid-diff`, and `:path` fence modifiers.

> [!IMPORTANT]
> **`Pennington.Roslyn` requires three package references**, not one. `Pennington.Roslyn` itself, `Microsoft.CodeAnalysis.Workspaces.MSBuild`, and `Microsoft.Build.Framework` (with runtime excluded). Skipping either of the last two leaves the MSBuild workspace unable to launch its out-of-process `BuildHost`, and every `csharp:xmldocid` fence renders an error comment instead of source. The full csproj fragment is in the next step.

<Steps>
<Step StepNumber="1">

**Add the three package references**

Add all three to the host csproj. `Pennington.Roslyn` brings in `SyntaxHighlighter` and `RoslynCodeBlockPreprocessor`; the other two are runtime requirements of `Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace`.

```xml
<PackageReference Include="Pennington.Roslyn" Version="0.1.0-alpha.0.20" />
<PackageReference Include="Microsoft.CodeAnalysis.Workspaces.MSBuild" Version="5.3.0" />
<PackageReference Include="Microsoft.Build.Framework" Version="18.4.0" ExcludeAssets="runtime" PrivateAssets="all" />
```

`Microsoft.CodeAnalysis.Workspaces.MSBuild` ships the `BuildHost-netcore/` content DLLs the workspace launches at solution-load time. Without it, every `csharp:xmldocid` fence renders an `<!-- Error processing xmldocid: … BuildHost.dll not found -->` comment. The `Microsoft.Build.Framework` reference (with runtime excluded) silences the MSBuild-locator resolution error without changing runtime behaviour.

</Step>
<Step StepNumber="2">

**Call `AddPenningtonRoslyn`**

Point it at the inner `.slnx`. That's the whole wire-up — no middleware call, no extra endpoint.

```csharp
builder.Services.AddPenningtonRoslyn(opts =>
    opts.SolutionPath = "path/to/your.slnx");
```

`SolutionPath` is resolved with `Path.GetFullPath`, so a relative value is interpreted against the **process working directory** — that is, the folder you run `dotnet run` from, which is normally the host csproj folder. The example string `"BeyondRoslynExample.slnx"` works because the inner `.slnx` sits next to the csproj. To point at a sibling folder, use a relative path like `"../OtherProject/Other.slnx"`; an absolute path also works.

</Step>
<Step StepNumber="3">

**See the options surface**

`RoslynOptions` carries `SolutionPath` (required for fence resolution) and `ProjectFilter` (narrows the workspace when the `.slnx` lists more than the docs need). For this tutorial, only `SolutionPath` matters. See <xref:reference.api.roslyn-options> for the full surface.

</Step>
<Step StepNumber="4">

**See the registration-only state**

Here is the stage 2 host: the same `AddDocSite` block as stage 1 plus one `AddPenningtonRoslyn` call. Nothing else changes.

```csharp:xmldocid,bodyonly,usings
M:BeyondRoslynExample.Stage2.Run(System.String[])
```

</Step>
</Steps>

### Checkpoint — The workspace loads at startup

- Run `dotnet run` on the host
- The first request takes a beat longer while `SolutionWorkspaceService` (`T:Pennington.Roslyn.Workspace.SolutionWorkspaceService`) loads the inner slnx
- No errors in the console — the workspace is hot and ready to resolve XmlDocIds

---

## 3. Write your first `xmldocid` fence

Now that `RoslynCodeBlockPreprocessor` (`T:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor`) is registered, any fenced code block whose info string ends in `:xmldocid` has its body parsed as one XmlDocId per line and resolved against the loaded workspace.

<Steps>
<Step StepNumber="1">

**Create a new markdown page**

Add `Content/api-pulls.md` with a front-matter block (`title`, `description`, `order`) and a heading. The next step fences a type from the Sample library into it.

</Step>
<Step StepNumber="2">

**Fence a whole type with `T:`**

The fence language is `csharp:xmldocid`. The body is a single XmlDocId — `T:` for a type, `M:` for a method, `P:` for a property, `F:` for a field.

```csharp:xmldocid
T:BeyondRoslynExample.Sample.Calculator
```

</Step>
<Step StepNumber="3">

**Fence a single method with `M:`**

Method XmlDocIds include full parameter types. The Sample library's `Add` method takes two `int` parameters, so the XmlDocId reads `M:...Add(System.Int32,System.Int32)`.

```csharp:xmldocid
M:BeyondRoslynExample.Sample.Calculator.Add(System.Int32,System.Int32)
```

</Step>
</Steps>

### Checkpoint — Real source renders inside the docs

- Run `dotnet run` and visit `http://localhost:5000/api-pulls`
- The `Calculator` class and the `Add` method render as syntax-highlighted C#, pulled directly from `Sample/Calculator.cs`
- Right-click → View Source: the markup is real `<pre><code>` with TextMate-style token spans, not an image

---

## 4. Watch hot reload refresh the snippet

The workspace re-reads source on change. Edit the fenced method, request the page again, and Pennington serves the updated snippet without a manual rebuild.

<Steps>
<Step StepNumber="1">

**Start the host in watch mode**

Run `dotnet watch` on the host csproj so file changes trigger a reload of the MSBuild workspace. Leave the browser open on `/api-pulls`.

</Step>
<Step StepNumber="2">

**Edit the Sample library**

Change the body of `Add` in `Sample/Calculator.cs` — add a comment or rename a local variable. Save the file.

</Step>
</Steps>

### Checkpoint — The page reflects the edit

- Refresh `/api-pulls`
- The `Add` method snippet now shows the change, pulled fresh from `Calculator.cs`
- No manual docs rebuild was required — the workspace picked it up

---

## 5. Use the `,bodyonly` variant and stack multiple symbols

Two fence options let you control what renders: append `,bodyonly` to strip the declaration line, or list multiple XmlDocIds in one fence to concatenate their source.

<Steps>
<Step StepNumber="1">

**Strip the declaration with `,bodyonly`**

Appending `,bodyonly` to the fence language returns only the block contents, or the expression-body expression for arrow members. Use it when the declaration is noise and the snippet should show what happens inside.

```csharp:xmldocid,bodyonly
M:BeyondRoslynExample.Sample.Calculator.Multiply(System.Int32,System.Int32)
```

</Step>
<Step StepNumber="2">

**Concatenate multiple XmlDocIds**

Place multiple XmlDocIds in one fence, one per line. The preprocessor renders them all in the order listed — useful for pairing two related members in the same code block.

```csharp:xmldocid
M:BeyondRoslynExample.Sample.Greeter.Greet(System.String)
M:BeyondRoslynExample.Sample.Calculator.Mean(System.Collections.Generic.IReadOnlyList{System.Int32})
```

</Step>
</Steps>

### Checkpoint — Both fence variants render

- Refresh `/api-pulls`
- The `Multiply` fence shows the `return a * b;` line only — no `public int Multiply(...)` declaration
- The concatenated fence shows `Greet` and `Mean` back-to-back in one highlighted code block

---

## Summary

- A dual-project shape now stands up — a DocSite host plus a sibling Sample library wired through an inner slnx.
- `Pennington.Roslyn` is active via a single `AddPenningtonRoslyn` call and `RoslynOptions.SolutionPath`.
- `csharp:xmldocid` fences cover types (`T:`), methods (`M:`), body-only snippets (`,bodyonly`), and multi-symbol blocks.
- Hot reload refreshes rendered snippets when the backing source changes.

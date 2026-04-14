---
title: "Connect to a Roslyn solution for live API snippets"
description: "Point Pennington at a .sln, pull method and class source into markdown with xmldocid fences, and watch hot reload refresh snippets when the source changes."
sectionLabel: Beyond the Basics
order: 20
tags: [roslyn, api-docs, xmldocid, hot-reload]
uid: tutorials.beyond-basics.connect-roslyn
---

> **In this page.** Point Pennington at a `.sln` via `SolutionPath`, use `xmldocid` code fences to pull method and class snippets straight from source, and let hot reload update the docs when the source changes.
>
> **Not in this page.** Generating full API-reference pages (that requires the planned surface beyond the current `Pennington.Roslyn` package) and authoring a custom Razor component for markdown — that is the next tutorial, [Author a custom Razor component for markdown](/tutorials/beyond-basics/custom-razor-component).

## What you'll do

_**Artifact** (one sentence): a running DocSite host that loads a sibling C# class library through an inner slnx and renders live `Calculator` and `Greeter` source inside a markdown page via `csharp:xmldocid` fences._

_**Skill** (one sentence): you'll know how to register `Pennington.Roslyn`, set `SolutionPath`, write the three fence variants (`:xmldocid`, `:xmldocid,bodyonly`, multi-symbol), and confirm that hot reload refreshes snippets when the source library changes._

## Prerequisites

_Keep this list to tools and prior tutorials only. The reader arrives with a working DocSite host from the docsite section — they're about to add one DI call and one sibling class library._

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](/tutorials/docsite/scaffold) (or have an equivalent DocSite host ready)
- A C# project or class library you want to fence into docs (we'll build a tiny one in unit 1)

The finished code for this tutorial lives in [`examples/BeyondRoslynExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondRoslynExample).

---

## 1. Give your host a sibling library to fence

_One sentence: before Pennington.Roslyn can pull source, there has to be a slnx that lists the project holding the types you want to embed — this unit stands up the dual-project shape the tutorial uses._

### Step 1.1 — Review the starting DocSite host

_One sentence of setup: this is the plain DocSite from the scaffold tutorial — no Roslyn wired yet. If you drop a `csharp:xmldocid` fence into a markdown page right now, it renders as a literal code block because no preprocessor is registered._

```csharp:xmldocid,bodyonly
M:BeyondRoslynExample.Stage1.Run(System.String[])
```

### Step 1.2 — Add a sibling `Sample` class library

_Drop a `Sample/BeyondRoslynExample.Sample.csproj` folder next to your host csproj. Set `GenerateDocumentationFile=true` so XmlDocId lookups resolve. Set `DefaultItemExcludes` on the host csproj to skip `Sample\**` — otherwise the two projects fight over the same `.cs` files._

### Step 1.3 — Add two small types to fence

_These are the lesson-scoped symbols the rest of the tutorial points at. The XML doc comments are what make them addressable by XmlDocId._

```csharp:xmldocid
T:BeyondRoslynExample.Sample.Calculator
```

```csharp:xmldocid
T:BeyondRoslynExample.Sample.Greeter
```

### Step 1.4 — Write an inner `BeyondRoslynExample.slnx`

_One line: create an inner slnx that registers only the Sample library. `SolutionPath` will point at this file, not at your outer repo-level slnx — that way the MSBuild workspace loads exactly the source you want to fence into docs and nothing else._

```text:path
examples/BeyondRoslynExample/BeyondRoslynExample.slnx
```

### Checkpoint — Two projects, one inner slnx

- Run `dotnet build` on both csprojs — they compile independently
- `BeyondRoslynExample.slnx` lives next to the host csproj and lists only `Sample/BeyondRoslynExample.Sample.csproj`
- Run `dotnet run` on the host — DocSite still serves, nothing has changed in the browser yet

---

## 2. Register `Pennington.Roslyn` and set `SolutionPath`

_One sentence: a single DI call turns on the xmldocid preprocessor — once `AddPenningtonRoslyn` runs with `SolutionPath` set, every markdown page in your content folder gains the `:xmldocid` / `:xmldocid,bodyonly` / `:xmldocid-diff` / `:path` fence modifiers._

### Step 2.1 — Add a package reference

_Reference `Pennington.Roslyn` from the host csproj. It brings in the MSBuild workspace, `SyntaxHighlighter`, and `RoslynCodeBlockPreprocessor`._

### Step 2.2 — Call `AddPenningtonRoslyn`

_Point it at the inner slnx. That's the whole wire-up — no middleware call, no extra endpoint._

```csharp:xmldocid
M:Pennington.Roslyn.RoslynExtensions.AddPenningtonRoslyn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Roslyn.RoslynOptions})
```

### Step 2.3 — See the options surface

_`RoslynOptions` carries `SolutionPath` (required for fence resolution) and `ProjectFilter` (narrow the workspace when the slnx has more than you need). For this tutorial only `SolutionPath` matters._

```csharp:xmldocid
T:Pennington.Roslyn.RoslynOptions
```

### Step 2.4 — See the registration-only state

_Here is the stage 2 host body: same `AddDocSite` block as stage 1 plus one `AddPenningtonRoslyn` call. Nothing else changes._

```csharp:xmldocid,bodyonly
M:BeyondRoslynExample.Stage2.Run(System.String[])
```

### Checkpoint — The workspace loads at startup

- Run `dotnet run` on the host
- The first request takes a beat longer while `SolutionWorkspaceService` (`T:Pennington.Roslyn.Workspace.SolutionWorkspaceService`) loads the inner slnx
- No errors in the console — the workspace is hot and ready to resolve XmlDocIds

---

## 3. Write your first `xmldocid` fence

_One sentence: now that `RoslynCodeBlockPreprocessor` (`T:Pennington.Roslyn.Preprocessing.RoslynCodeBlockPreprocessor`) is registered, any fenced code block whose info string ends in `:xmldocid` has its body parsed as one XmlDocId per line and resolved against the loaded workspace._

### Step 3.1 — Create a new markdown page

_Add `Content/api-pulls.md` with a front-matter block (`title`, `description`, `order`) and a heading. You're about to fence a type from the Sample library into it._

### Step 3.2 — Fence a whole type with `T:`

_The fence language is `csharp:xmldocid`. The body is a single XmlDocId — `T:` for a type, `M:` for a method, `P:` for a property, `F:` for a field._

```csharp:xmldocid
T:BeyondRoslynExample.Sample.Calculator
```

### Step 3.3 — Fence a single method with `M:`

_Method XmlDocIds include parameter types. The Sample library's `Add` method takes two `int` parameters, so the XmlDocId reads `M:...Add(System.Int32,System.Int32)`._

```csharp:xmldocid
M:BeyondRoslynExample.Sample.Calculator.Add(System.Int32,System.Int32)
```

### Checkpoint — Real source renders inside your docs

- Run `dotnet run` and visit `http://localhost:5000/api-pulls`
- You should see the `Calculator` class and the `Add` method rendered as syntax-highlighted C#, pulled directly from `Sample/Calculator.cs`
- Right-click → View Source: the markup is real `<pre><code>` with TextMate-style token spans, not an image

---

## 4. Watch hot reload refresh the snippet

_One sentence: the workspace re-reads source on change — edit the fenced method, request the page again, and Pennington serves the updated snippet without a rebuild._

### Step 4.1 — Start the host in watch mode

_Run `dotnet watch` on the host csproj so file changes trigger a reload of the MSBuild workspace. Leave the browser open on `/api-pulls`._

### Step 4.2 — Edit the Sample library

_Change the body of `Add` in `Sample/Calculator.cs` — e.g., add a comment or rename a local. Save the file._

### Checkpoint — The page shows your edit

- Refresh `/api-pulls`
- The `Add` method snippet now shows your change, pulled fresh from `Calculator.cs`
- You never rebuilt the docs site manually — the workspace picked it up

---

## 5. Use the `,bodyonly` variant and stack multiple symbols

_One sentence: two fence-body flourishes let you control what renders — append `,bodyonly` to strip the declaration line, or list multiple XmlDocIds in one fence to concatenate their source._

### Step 5.1 — Strip the declaration with `,bodyonly`

_Appending `,bodyonly` to the fence language returns just the block contents (or the expression-body expression). Useful when the declaration is noise and you only want "what happens inside"._

```csharp:xmldocid,bodyonly
M:BeyondRoslynExample.Sample.Calculator.Multiply(System.Int32,System.Int32)
```

### Step 5.2 — Concatenate multiple XmlDocIds

_One fence, multiple XmlDocIds, one per line. The preprocessor renders them all in the order you list them — handy when you want two related members in the same code block._

```csharp:xmldocid
M:BeyondRoslynExample.Sample.Greeter.Greet(System.String)
M:BeyondRoslynExample.Sample.Calculator.Mean(System.Collections.Generic.IReadOnlyList{System.Int32})
```

### Checkpoint — Both fence variants render

- Refresh `/api-pulls`
- The `Multiply` fence shows the `return a * b;` line only — no `public int Multiply(...)` declaration
- The concatenated fence shows `Greet` and `Mean` back-to-back in one highlighted code block

---

## Summary

- You stood up a dual-project shape — a DocSite host plus a sibling Sample library wired through an inner slnx.
- You turned on `Pennington.Roslyn` with a single `AddPenningtonRoslyn` call and `RoslynOptions.SolutionPath`.
- You wrote `csharp:xmldocid` fences for types (`T:`), methods (`M:`), body-only snippets (`,bodyonly`), and multi-symbol blocks.
- You confirmed hot reload refreshes rendered snippets when the backing source changes.

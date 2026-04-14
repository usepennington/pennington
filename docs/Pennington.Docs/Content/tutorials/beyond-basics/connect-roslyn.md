---
title: "Connect to a Roslyn solution for live API snippets"
description: "Point Pennington at a .sln so markdown code fences can pull live method and class snippets from your source via xmldocid."
section: "beyond-basics"
order: 20
tags: []
uid: tutorials.beyond-basics.connect-roslyn
isDraft: true
search: false
llms: false
---

> **In this page.** Pointing Pennington at a `.sln` via `SolutionPath`, using `xmldocid` code fences (e.g., ` ```csharp:xmldocid ` on one line followed by the id `M:Ns.Type.Method`) to pull method/class snippets straight from source, and letting hot reload update the docs when the source changes.
>
> **Not in this page.** Generating full API-reference pages — that requires the planned `Pennington.Roslyn` package.

## What you'll do

- **Artifact**: a Pennington docs site whose markdown pages render live C# snippets pulled directly from a sibling solution, re-rendered the moment the source file is saved.
- **Skill**: you'll know how to wire `AddPenningtonRoslyn` to a `.sln`, author `xmldocid` fences against real symbols, and use `bodyonly` to trim declaration noise.

## Prerequisites

- .NET 11 SDK installed
- Completed [Build your first Pennington site](/tutorials/getting-started/first-site) (or have a running Pennington site with at least one markdown source)
- A sibling C# project in the same repo that contains the symbols you want to embed (this tutorial uses `examples/Spectre.Console.Examples/` as that project)

The finished code for this tutorial lives in [`examples/SpectreConsoleExample`](https://github.com/usepennington/pennington/tree/main/examples/SpectreConsoleExample), which pulls snippets from the sibling [`examples/Spectre.Console.Examples`](https://github.com/usepennington/pennington/tree/main/examples/Spectre.Console.Examples) project.

---

## 1. Add the Roslyn package and point it at a solution

- One-sentence framing: Pennington's core pipeline doesn't know about C# symbols; the optional `Pennington.Roslyn` package layers on a solution workspace, support for `xmldocid` fences, and a Roslyn-powered highlighter.
- We'll land on a working `Program.cs` that calls `AddPennington` and then `AddPenningtonRoslyn` with a `SolutionPath` pointing at our own `.sln`.

### Step 1.1 — Reference the Pennington.Roslyn package

- The package reference is added to the docs-site `.csproj` alongside the existing `Pennington` reference.
- No code changes yet — this step is purely the `dotnet add package` so the using directives resolve.
- Run `dotnet add package Pennington.Roslyn` from the docs-site folder.

### Step 1.2 — Wire AddPenningtonRoslyn in Program.cs

- Call `builder.Services.AddPenningtonRoslyn(...)` after `AddPennington` and set `options.SolutionPath` to a path relative to the site's content root.
- This turns on the solution workspace, symbol resolution for `xmldocid` fences, and the Roslyn highlighter so plain C# fences also get semantic colouring.
- Reference the sibling solution using a relative path from the docs project (e.g., `"../../Pennington.slnx"`).

```csharp:xmldocid
T:RoslynIntegrationExample.BlogFrontMatter
```

- Snippet source: `examples/RoslynIntegrationExample/BlogFrontMatter.cs` — shows the minimal front-matter record used by the example that has `AddPenningtonRoslyn` wired in its `Program.cs`. The `Program.cs` itself is top-level statements so it has no xmldocid; this record anchors the project we're pointing at.

### Checkpoint — Roslyn is active

- Run `dotnet run` and open any markdown page that contains a plain ` ```csharp` fence.
- In the browser, view source on a code block: C# keywords should now be wrapped in highlighter spans emitted by the Roslyn highlighter rather than the TextMate fallback.
- If `SolutionPath` is wrong the dev overlay shows a diagnostic warning — fix the path before continuing.

---

## 2. Embed a method snippet with an xmldocid fence

- One-sentence framing: the `xmldocid` fence tells Pennington to look up a symbol by its documentation ID and splice the source text into the rendered page.
- We'll add one fence that targets a single method in the sibling project.

### Step 2.1 — Pick a method and write the fence

- Open a markdown file in `Content/` (any page will do) and add a fenced block using the form ` ```csharp:xmldocid` with the symbol ID on the next line.
- Method IDs start with `M:`, fully-qualified by namespace, type, and parameter list.
- When the ID resolves, Pennington reads the source from the workspace and emits the full method declaration including signature and body.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowDataTable
```

- Snippet source: `examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs` — demonstrates a real method the `SpectreConsoleExample` docs pull in to show how to build a Spectre.Console `Table` with coloured column headers and four data rows.

### Step 2.2 — Verify the fence resolved

- Save the markdown file and watch the terminal: the workspace loads on first hit, so look for the solution-load line in the output.
- Reload the page — you should see the method body rendered as highlighted C#.
- If the fence renders as a comment like `<!-- Unresolved xmldocid: ... -->` the symbol ID is wrong; cross-check the namespace and parameter signature (generic methods use ``` ``1 ``` and backticks for arity).

### Checkpoint — one live method on the page

- A visit to the markdown page shows the `ShowDataTable` method body in-line, with keywords, types, and string literals coloured.
- The diagnostic overlay is clean (no `Unresolved xmldocid` warnings).

---

## 3. Embed a full class and trim declaration noise

- One-sentence framing: `T:` prefixes let you pull a whole class, and the `bodyonly` modifier strips the outer declaration when you want just the members.
- We'll add two more fences to demonstrate both shapes side by side.

### Step 3.1 — Add a type-level fence

- Use the `T:` prefix followed by the fully-qualified type name.
- Pennington returns the entire class declaration including XML doc comments if present.

```csharp:xmldocid
T:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample
```

- Snippet source: `examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs` — demonstrates a full `IExample` implementation (`Run` plus the four `ShowX` helper methods) as one page-sized code block.

### Step 3.2 — Add a second method fence with bodyonly

- Append `,bodyonly` after `xmldocid` (so the fence info-string reads `csharp:xmldocid,bodyonly`) to drop the method signature line and braces, leaving only the statements inside.
- Useful when the method signature is obvious from surrounding prose and the declaration would be visual noise.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowColoredHelloWorld
```

- Snippet source: `examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs` — the short `ShowColoredHelloWorld` method that prints coloured markup greetings; small enough to read at a glance with `bodyonly` applied.

### Checkpoint — three shapes on one page

- The page now contains three distinct fences: a `M:` fence rendering a full method, a `T:` fence rendering a whole class, and an `M:…,bodyonly` fence rendering just the statements inside a method.
- All three render with the same highlighted appearance as hand-written ` ```csharp` blocks.

---

## 4. Edit the source and watch the docs update

- One-sentence framing: the Roslyn workspace watches your source files, so changing a `.cs` file invalidates the cached workspace and forces the next render to re-read the symbol.
- This is the same live-reload path the rest of Pennington uses for markdown, so no extra configuration is needed.

### Step 4.1 — Run under dotnet watch

- Stop the site and start it again with `dotnet watch` so both Razor and file-watched Pennington services re-flow on change.
- Leave the browser open to the page with your fences.

### Step 4.2 — Edit the method in the sibling project

- Open `examples/Spectre.Console.Examples/console/tutorials/GettingStartedExample.cs` in your editor.
- Change a string literal inside `ShowColoredHelloWorld` and save.
- The browser live-reload connection fires; the page re-renders with the new text pulled from the updated source.

### Checkpoint — the edit round-trips

- Without restarting the host, the page now reflects the new string literal.
- The terminal shows the Roslyn workspace reloading the affected project.
- Revert the edit to leave the example project clean.

---

## Summary

- You connected a docs site to a real `.sln` with `AddPenningtonRoslyn` + `SolutionPath`, turning on the workspace, symbol lookup, and Roslyn-powered highlighting.
- You can embed any C# method or type in a markdown page via a `csharp:xmldocid` fence and pick between the full declaration and `bodyonly` form.
- You rely on Pennington's file watching to keep rendered snippets in sync with the source without restarting the host.
- You know how to read the dev overlay for `Unresolved xmldocid` diagnostics and correct symbol IDs when they drift.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.

---
title: "Author a custom Razor component for markdown"
description: "Author a PricingCard Razor component inside a DocSite, register it with AddMdazorComponent<T>(), and render it from a markdown page with two parameter sets."
sectionLabel: "Beyond the Basics"
order: 104030
tags:
  - razor
  - mdazor
  - components
  - beyond-basics
uid: tutorials.beyond-basics.custom-razor-component
---

By the end of this tutorial you'll have a running DocSite at `http://localhost:5000/pricing` that renders two styled `<PricingCard />` cards — a standard "Basic" tier and a highlighted "Pro" tier — both driven by tag attributes inside a plain markdown file.

Along the way, the tutorial covers authoring a Razor component with `[Parameter]`-decorated properties, wiring it into Mdazor's component registry with one `AddMdazorComponent<T>()` line, and consuming it from markdown with self-closing tag syntax whose attribute values bind case-insensitively to the component's parameters.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) (provides the `AddDocSite` / `UseDocSite` / `RunDocSiteAsync` host shape this tutorial extends)
- Completed [Connect to a Roslyn solution for live API snippets](xref:tutorials.beyond-basics.connect-roslyn) (previous tutorial in the Beyond the Basics section)
- Basic Razor familiarity — a `.razor` file with `@code {}` and `[Parameter]` properties should feel routine

The finished code for this tutorial lives in [`examples/BeyondCustomRazorComponentExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondCustomRazorComponentExample).

---

## 1. Author the PricingCard component

Before Mdazor can render a custom tag from markdown, a real Razor component has to exist in the project. This unit adds `Components/PricingCard.razor` and a top-level `_Imports.razor` so `[Parameter]` is in scope without per-file `@using` lines.

### Step 1.1 — Add a project-wide `_Imports.razor`

Drop an `_Imports.razor` file at the project root so every `.razor` file in the project gets the Blazor component namespaces. This is the same file a Blazor template ships with — the two `@using` lines are what make `[Parameter]` resolve inside the component file in the next step.

```razor:path
examples/BeyondCustomRazorComponentExample/_Imports.razor
```

### Step 1.2 — Create `Components/PricingCard.razor`

Create a `Components/` folder and add `PricingCard.razor` with four `[Parameter]` properties — `Tier`, `Price`, `Features`, and `Highlighted` — and markup that renders a pricing card with a "Most Popular" badge when highlighted. The `Features` parameter is a pipe-delimited string because Mdazor binds only primitive parameter types from markdown attributes; lists arrive as strings and are split inside the component.

```csharp:xmldocid,bodyonly
M:BeyondCustomRazorComponentExample.Stage1.Source
```

The file is a regular Blazor component — there is nothing Pennington-specific about it yet. Mdazor discovers it in the next unit.

### Checkpoint — The component compiles but markdown cannot see it

Run `dotnet build` from `examples/BeyondCustomRazorComponentExample`. The build succeeds and produces `BeyondCustomRazorComponentExample.dll`. The `PricingCard` type exists at `BeyondCustomRazorComponentExample.Components.PricingCard` but is not yet wired to Mdazor, so a `<PricingCard />` tag in markdown renders as a literal custom element.

---

## 2. Register the component with Mdazor

DocSite already calls `AddMdazor()` and registers the built-in Pennington.UI components. The only remaining step is one `AddMdazorComponent<PricingCard>()` line so Mdazor's registry knows about the new type.

### Step 2.1 — Add `AddMdazorComponent<PricingCard>()` to `Program.cs`

Open `Program.cs` and add a single `builder.Services.AddMdazorComponent<PricingCard>()` line after the `AddDocSite` block. The extension lives in the `Mdazor` namespace and ships from the `Mdazor` NuGet package, already transitively referenced through `Pennington.DocSite` — no package add required.

```csharp:xmldocid,bodyonly
M:BeyondCustomRazorComponentExample.Stage2.Run(System.String[])
```

`AddMdazorComponent<T>()` returns `IServiceCollection`, so additional component registrations can chain off the same call. That becomes handy when registering several custom components at once.

### Step 2.2 — Confirm the host still boots

Run the DocSite host to verify the extra DI line did not break startup. No markdown change has been made yet, so the site renders exactly as it did before — the new wiring stays invisible until a page consumes the tag.

### Checkpoint — Mdazor knows about PricingCard

Run `dotnet run` from `examples/BeyondCustomRazorComponentExample` and visit `http://localhost:5000/`. The landing page renders without errors and the log shows the site serving on port 5000. `PricingCard` is now a registered Mdazor component; rendering is proved in the next unit.

---

## 3. Consume the component from markdown

Now let's add a markdown page that uses `<PricingCard />` twice with different attribute values, exercising both the default and highlighted visual states of the component.

### Step 3.1 — Create `Content/pricing.md`

Add a new markdown page under `Content/` with front matter (`title: Pricing`, `description:`, `order: 20`) and two `<PricingCard ... />` tags between headings. The first card uses `Tier="Basic" Price="9"`; the second adds `Highlighted="true"` and richer feature text.

```markdown:path
examples/BeyondCustomRazorComponentExample/Content/pricing.md
```

Two rules govern how the page works. Tag-name matching is case-sensitive on the leading character — `<PricingCard>` must start with a capital letter to be treated as a component candidate. Attribute-to-parameter binding is case-insensitive via reflection, so `Tier="Pro"` binds to `[Parameter] public string Tier` regardless of casing.

### Step 3.2 — Refresh the pricing page in the browser

With `dotnet run` still active, open `http://localhost:5000/pricing`. Mdazor intercepts each `<PricingCard ... />` tag, looks up the registered type, instantiates it, assigns parameters from the attributes, renders the component through Blazor's server-side `HtmlRenderer`, and inlines the resulting HTML back into the Markdig pipeline's output.

### Checkpoint — Two cards render on the pricing page

Visit `http://localhost:5000/pricing`. Two pricing cards appear: a plain **Basic** card at `$9 / month` and a **Pro** card at `$49 / month` with a "Most Popular" pill and a thicker accent border. View the page source — `<PricingCard>` has been replaced by real HTML (a `<div>` tree with the card classes), not left as a literal custom element.

---

## 4. Pass more parameters and verify binding

Now let's confirm the markdown-to-parameter binding is real by editing attribute values in the markdown and watching the rendered output change — this is the whole authoring loop.

### Step 4.1 — Edit the Pro card to change `Price` and `Features`

In `Content/pricing.md`, change `Price="49"` to `Price="99"` and extend the `Features=""` string with an extra pipe-separated entry (for example, `"...|24/7 chat support"`). Save the file.

### Step 4.2 — Flip `Highlighted` on the Basic card

Add `Highlighted="true"` to the first `<PricingCard Tier="Basic" ... />` tag. Boolean attribute values from markdown bind with case-insensitive `true` / `false` — `Highlighted="True"` and `Highlighted="true"` both flip the card into its emphasised state.

### Checkpoint — The rendered cards reflect the edits

Reload `http://localhost:5000/pricing`. The dev host picks up markdown changes as you save, so no rebuild is required.

- The Pro card now reads **$99 / month** and lists the extra feature bullet
- The Basic card now has the "Most Popular" pill and the highlighted border
- Open the browser's dev tools — the generated HTML under each `<PricingCard>` has changed to match

---

## Summary

- A Razor component lives under `Components/` with `[Parameter]`-decorated properties and is consumed from markdown by name.
- Any component type registers with Mdazor in one line: `services.AddMdazorComponent<T>()` after `AddDocSite` (or after `AddPennington` on a custom host).
- Two binding rules govern markdown-driven consumption: tag names start with a capital letter, and attribute values bind case-insensitively to parameter properties of primitive types (`string`, `bool`, numbers).
- Built-in Pennington.UI components and custom components mix freely in the same markdown page — both go through the same Mdazor registry.

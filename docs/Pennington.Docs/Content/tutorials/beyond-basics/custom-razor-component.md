---
title: "Author a custom Razor component for markdown"
description: "Author a PricingCard Razor component inside a DocSite, register it with AddMdazorComponent<T>(), and render it from a markdown page with two parameter sets."
sectionLabel: "Beyond the Basics"
order: 30
tags:
  - razor
  - mdazor
  - components
  - beyond-basics
uid: tutorials.beyond-basics.custom-razor-component
---

> **In this page.** _One sentence paraphrasing the Covers line: the reader builds a `PricingCard.razor` component under `Components/`, registers it with `services.AddMdazorComponent<PricingCard>()`, and consumes it twice from a markdown page so Mdazor binds the attributes to `[Parameter]` properties and inlines the rendered HTML into the Pennington site._
>
> **Not in this page.** _One sentence paraphrasing the Does-not-cover line: point the reader at [Use UI components inside markdown](xref:how-to.content-authoring.ui-components-in-markdown) for the built-in Pennington.UI components and at the reference pages under `/reference/ui/` for full component APIs — packaging reusable Razor component libraries and the full internals of Mdazor's parser are out of scope here._

## What you'll do

_**Artifact** (one sentence): a running DocSite at `http://localhost:5000/pricing` that renders two styled `<PricingCard />` cards — a standard "Basic" tier and a highlighted "Pro" tier — both driven by tag attributes inside a plain markdown file._

_**Skill** (one sentence): the reader walks away knowing how to author a Razor component with `[Parameter]`-decorated properties, wire it into Mdazor's component registry with one `AddMdazorComponent<T>()` line, and consume it from markdown with self-closing tag syntax whose attribute values bind case-insensitively to the component's parameters._

## Prerequisites

_Keep this list to tools and prior tutorials. Razor familiarity is assumed — this tutorial does not teach `@code`, `[Parameter]`, or Blazor render semantics. DocSite hosting is identical to the scaffolding tutorial so link back rather than re-explain._

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) (provides the `AddDocSite` / `UseDocSite` / `RunDocSiteAsync` host shape this tutorial extends)
- Completed [Connect to a Roslyn solution for live API snippets](xref:tutorials.beyond-basics.connect-roslyn) (previous tutorial in the Beyond the Basics section)
- Basic Razor familiarity — you have written a `.razor` file with `@code {}` and `[Parameter]` properties before

The finished code for this tutorial lives in [`examples/BeyondCustomRazorComponentExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondCustomRazorComponentExample).

---

## 1. Author the PricingCard component

_One sentence: before Mdazor can render a custom tag from markdown, a real Razor component has to exist in the project — this unit adds `Components/PricingCard.razor` and a top-level `_Imports.razor` so `[Parameter]` is in scope without per-file `@using` lines._

### Step 1.1 — Add a project-wide `_Imports.razor`

_Drop an `_Imports.razor` file at the project root so every `.razor` file in the project gets the Blazor component namespaces. This is the same file a Blazor template ships with — the two `@using` lines are what make `[Parameter]` resolve inside the component file in the next step._

```razor:path
examples/BeyondCustomRazorComponentExample/_Imports.razor
```

### Step 1.2 — Create `Components/PricingCard.razor`

_Create a `Components/` folder and add `PricingCard.razor` with four `[Parameter]` properties (`Tier`, `Price`, `Features`, `Highlighted`) and markup that renders a pricing card with a "Most Popular" badge when highlighted. The `Features` parameter is a pipe-delimited string — Mdazor only binds primitive parameter types from markdown attributes, so lists arrive as strings and are split inside the component._

```csharp:xmldocid,bodyonly
M:BeyondCustomRazorComponentExample.Stage1.Source
```

_One-line callout: the file is a regular Blazor component — there is nothing Pennington-specific about it yet. Mdazor discovers it in the next unit._

### Checkpoint — The component compiles but markdown does not see it

_Concrete verification. The project builds, but a `<PricingCard />` tag in markdown would render as a literal custom element because Mdazor has not been told about the type._

- Run `dotnet build` from `examples/BeyondCustomRazorComponentExample`
- The build succeeds and produces `BeyondCustomRazorComponentExample.dll`
- The `PricingCard` type exists at `BeyondCustomRazorComponentExample.Components.PricingCard` but is not yet wired to Mdazor

---

## 2. Register the component with Mdazor

_One sentence: DocSite already calls `AddMdazor()` and registers the built-in Pennington.UI components — the reader's only job is to add one `AddMdazorComponent<PricingCard>()` line so Mdazor's registry knows about the new type._

### Step 2.1 — Add `AddMdazorComponent<PricingCard>()` to `Program.cs`

_Open `Program.cs` and add a single `builder.Services.AddMdazorComponent<PricingCard>()` line after the `AddDocSite` block. The extension lives in the `Mdazor` namespace and is shipped by the `Mdazor` NuGet package, which is already transitively referenced through `Pennington.DocSite` — no package add is required._

```csharp:xmldocid,bodyonly
M:BeyondCustomRazorComponentExample.Stage2.Run(System.String[])
```

_Explain the one non-obvious detail: `AddMdazorComponent<T>()` returns `IServiceCollection`, so additional component registrations can chain off the same call — useful later when you register several custom components at once._

### Step 2.2 — Confirm the host still boots

_Run the DocSite host to verify the extra DI line did not break startup. No markdown change has been made yet, so the site renders exactly as it did before — the new wiring is invisible until a page consumes the tag._

### Checkpoint — Mdazor knows about PricingCard

_Concrete verification. The host runs; `PricingCard` is now a registered Mdazor component. Rendering is proved in the next unit._

- Run `dotnet run` from `examples/BeyondCustomRazorComponentExample`
- Visit `http://localhost:5000/` and confirm the landing page renders without errors
- The log line shows the site serving on `http://localhost:5000`

---

## 3. Consume the component from markdown

_One sentence: add a `Content/pricing.md` page that uses `<PricingCard />` twice with different attribute values, exercising both the default and highlighted visual states of the component._

### Step 3.1 — Create `Content/pricing.md`

_Add a new markdown page under `Content/` with front matter (`title: Pricing`, `description:`, `order: 20`) and two `<PricingCard ... />` tags between headings. The first card uses `Tier="Basic" Price="9"`; the second adds `Highlighted="true"` and richer feature text._

```markdown:path
examples/BeyondCustomRazorComponentExample/Content/pricing.md
```

_Explain the two load-bearing rules the page exercises: tag-name matching is **case-sensitive on the leading character** (`<PricingCard>` must start with a capital to be treated as a component candidate), and attribute-to-parameter binding is **case-insensitive via reflection** (so `Tier="Pro"` binds to `[Parameter] public string Tier`)._

### Step 3.2 — Refresh the pricing page in the browser

_With `dotnet run` still active, open `http://localhost:5000/pricing`. Mdazor intercepts each `<PricingCard ... />` tag, looks up the registered type, instantiates it, assigns parameters from the attributes, renders the component through Blazor's server-side `HtmlRenderer`, and inlines the resulting HTML back into the Markdig pipeline's output._

### Checkpoint — Two cards render on the pricing page

_Concrete verification. Both parameter variants are visible in the browser._

- Visit `http://localhost:5000/pricing`
- You see two pricing cards: a plain **Basic** card at `$9 / month` and a **Pro** card at `$49 / month` with a "Most Popular" pill and a thicker accent border
- View the page source — `<PricingCard>` has been replaced by real HTML (a `<div>` tree with the card classes), not left as a literal custom element

---

## 4. Pass more parameters and verify binding

_One sentence: prove the Markdown-to-parameter binding is real by editing attribute values in the markdown and watching the rendered output change — this is the whole authoring loop the reader now owns._

### Step 4.1 — Edit the Pro card to change `Price` and `Features`

_In `Content/pricing.md`, change `Price="49"` to `Price="99"` and extend the `Features=""` string with an extra pipe-separated entry (for example, `"...|24/7 chat support"`). Save the file._

### Step 4.2 — Flip `Highlighted` on the Basic card

_Add `Highlighted="true"` to the first `<PricingCard Tier="Basic" ... />` tag. Boolean attribute values from markdown bind with case-insensitive `true` / `false` — `Highlighted="True"` and `Highlighted="true"` both flip the card into its emphasised state._

### Checkpoint — The rendered cards reflect the edits

_Concrete verification. The browser shows the new attribute values without requiring a rebuild — the dev host picks up markdown changes as you save._

- Reload `http://localhost:5000/pricing`
- The Pro card now reads **$99 / month** and lists the extra feature bullet
- The Basic card now has the "Most Popular" pill and the highlighted border
- Open the browser's dev tools — the generated HTML under each `<PricingCard>` has changed to match

---

## Summary

_Three to five bullets. Each one names a capability the reader now has, not a topic the page covered._

- You can author a Razor component under `Components/` with `[Parameter]`-decorated properties and use it from markdown by name.
- You can register any component type with Mdazor in one line: `services.AddMdazorComponent<T>()` after `AddDocSite` (or after `AddPennington` on a custom host).
- You know the two binding rules for markdown-driven consumption: tag names must start with a capital letter, and attribute values bind case-insensitively to parameter properties of primitive types (`string`, `bool`, numbers).
- You can mix built-in Pennington.UI components and your own custom components in the same markdown page — both go through the same Mdazor registry.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.

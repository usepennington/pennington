---
title: "Author a custom Razor component for your content"
description: "Create a custom Razor component for Pennington's Mdazor-based markdown flow, keep the Pennington-specific setup simple, and link to Mdazor for the deeper syntax and parser behavior."
section: "beyond-basics"
order: 30
tags: []
uid: tutorials.beyond-basics.custom-razor-component
isDraft: true
search: false
llms: false
---

> **In this page.** Creating a Razor component for use with Pennington's Mdazor-based markdown-component flow, exposing simple parameters and `ChildContent`, and keeping the Pennington-specific setup clear.
>
> **Not in this page.** Writing an `IIslandRenderer` for SPA-interactive regions, authoring brand-new Markdig extensions, or re-documenting Mdazor internals and edge cases in full.

> **Scope note.** Pennington's component-tags-in-markdown story is based on [Mdazor](https://github.com/phil-scott-78/Mdazor). This tutorial should stay Pennington-specific: create a component that fits the docs site, keep its parameters simple, and point readers to Mdazor for the deeper rules around tag syntax, registration mechanics, nested markdown, unknown-component fallback, and limitations.

## What you'll do

- **Artifact:** a custom Razor component shaped for use in Pennington docs content, with simple parameters and a `ChildContent` slot.
- **Skill:** you'll know how to design a markdown-friendly component surface for Pennington and when to send readers to Mdazor for the underlying parser details.

## Prerequisites

- .NET 11 SDK installed
- Completed the getting-started path through [Style the site with MonorailCSS](/tutorials/getting-started/styling) and at least one DocSite or BlogSite scaffold tutorial
- A running site where the Mdazor-based markdown-component flow is available

Use the [Mdazor project](https://github.com/phil-scott-78/Mdazor) as the external reference for the underlying component-tag syntax and limitations.

---

## 1. Create the component file

- Razor components in a Pennington site live under `Components/` (or any folder your project uses by convention).
- Keep the public surface small: simple scalar parameters plus `ChildContent` are the easiest shape for markdown authors.
- Use the Pennington.UI components as style references; use Mdazor as the external reference for how component tags behave in markdown.

### Step 1.1 — Add `Components/Callout.razor`

- Create `Components/Callout.razor` in your site project.
- Declare two parameters (`Tone` and `Title`) plus a `ChildContent` fragment.
- Render a simple card-shaped block with a border and padding.

```razor
@* Components/Callout.razor *@

<div class="my-4 rounded-lg border-l-4 border-@ToneColor-500 bg-@ToneColor-50 dark:bg-@ToneColor-950/40 p-4 not-prose">
    @if (!string.IsNullOrWhiteSpace(Title))
    {
        <p class="font-display font-semibold text-@ToneColor-800 dark:text-@ToneColor-200">@Title</p>
    }
    <div class="text-sm text-base-700 dark:text-base-300">
        @ChildContent
    </div>
</div>

@code {
    [Parameter] public string Tone { get; set; } = "note";
    [Parameter] public string? Title { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string ToneColor => Tone switch
    {
        "success" => "emerald",
        "tip" => "sky",
        "caution" => "amber",
        "danger" => "rose",
        _ => "base",
    };
}
```

### Step 1.2 — Make it visible to the markdown-component flow

- Import the component namespace where your markdown-component setup expects it.
- Keep the registration or discovery step close to the rest of your markdown-component configuration.
- Link to Mdazor rather than reproducing all of its registration details here.

### Checkpoint — the component compiles

- Run `dotnet build` and confirm the component compiles.
- The component is ready to be used from markdown once the site wiring is in place.

---

## 2. Use it from markdown with simple parameters

- The markdown author should be able to discover the component from its tag name and a few obvious attributes.
- Start with one simple use case, such as a callout box or status panel.
- Keep the syntax examples short here; send readers to Mdazor for the full grammar and nesting behavior.

### Step 2.1 — Add a simple usage example to a markdown page

- Show the component used with a couple of simple attributes and short child content.
- Keep the example close to the way a docs author would actually type it in markdown.

```razor
<Callout Tone="tip" Title="Heads up">
    This section is being rewritten this sprint — expect small churn.
</Callout>
```

### Step 2.2 — Keep the parameters markdown-friendly

- Prefer string, number, and bool parameters first.
- Avoid starting with complex types or clever APIs.
- Call out that Mdazor currently has its own limits around parameter types and registration, and link there instead of unpacking them here.

### Checkpoint — the component has a usable authoring surface

- The component name is clear
- The parameters are easy to understand
- A markdown author can copy the example and adapt it

---

## 3. Design the child-content slot

- `ChildContent` is usually the right shape when the component wraps arbitrary prose.
- Keep the surrounding component lightweight so the markdown inside still does most of the content work.
- If readers need the full nested-markdown behavior, link them to Mdazor instead of restating it here.

### Step 3.1 — Show nested content

- Update the call site from step 2 to include a nested `<Badge>`:

```razor
<Callout Tone="caution" Title="Deprecation">
    The legacy <Badge Text="v1" Variant="danger" Size="small" /> API will be removed in the next release.
</Callout>
```

### Step 3.2 — Link out for the deeper behavior

- Confirm the nested content example is easy to read.
- Add a short note pointing to [Mdazor](https://github.com/phil-scott-78/Mdazor) for the deeper rules around nested markdown, component registration, and current limitations.

### Checkpoint — the component is ready to document

- The component has a clear tag name
- The parameters are simple
- The outline knows to link to Mdazor for the deeper details

---

## Summary

- You created a markdown-friendly Razor component for Pennington docs content
- You kept its public surface simple and easy to author
- You used Pennington-specific guidance for the component shape
- You linked readers to Mdazor for the underlying syntax, parser behavior, and limitations

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.

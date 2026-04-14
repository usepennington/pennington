---
title: "Use UI components inside markdown"
description: "Use Pennington.UI components inside markdown through Pennington's Mdazor-based component support."
section: "content-authoring"
order: 90
tags: []
uid: how-to.content-authoring.ui-components-in-markdown
isDraft: true
search: false
llms: false
---

> **In this page.** Using Pennington.UI components such as `<Steps>` / `<Step>`, `<Card>`, `<CardGrid>`, `<LinkCard>`, and `<Badge>` inside markdown through Pennington's Mdazor-based component support.
>
> **Not in this page.** The underlying Mdazor tag syntax, nesting rules, and parser limitations — see the [Mdazor project](https://github.com/phil-scott-78/Mdazor) for those.

> **Scope note.** Pennington's component-tags-in-markdown story is based on [Mdazor](https://github.com/phil-scott-78/Mdazor), a Markdig extension for rendering Razor components inside markdown. This page stays Pennington-specific: which built-in components are useful, which parameters matter in practice, and where to link out for deeper syntax details.

## When to use this

When plain markdown cannot express the UI block you need — ordered procedures with chrome, framed callouts, card grids, or navigation tiles — reach for one of the Pennington.UI components directly inside the markdown body.

## Assumptions

- You have an existing Pennington site using either `AddDocSite`, `AddBlogSite`, or a custom `AddPennington` setup.
- The site has the Mdazor-based markdown-component path enabled.
- You can import `Pennington.UI.Components` wherever those component tags need to resolve.

---

## Steps

### 1. Make the component tags available

Import `Pennington.UI.Components` where your markdown-component setup expects those tags to resolve. Keep this import local to the site project so the markdown authoring surface stays predictable.

```razor
@using Pennington.UI.Components
```

### 2. Use `<Steps>` and `<Step>` for ordered procedures

Reach for `<Steps>` when a procedure needs more structure than a plain numbered markdown list. Parameters:

- `Steps`: `Type` (`string`, default `"primary"`), `ChildContent`.
- `Step`: `StepNumber` (`string`, default `"1"`), `ChildContent`.

```razor
<Steps>
    <Step StepNumber="1">Install the NuGet package.</Step>
    <Step StepNumber="2">Call <code>AddPennington</code> in <code>Program.cs</code>.</Step>
    <Step StepNumber="3">Call <code>UsePennington</code> on the <code>WebApplication</code>.</Step>
</Steps>
```

### 3. Use `<Card>` for titled callouts and framed asides

Renders a titled rounded card with an optional leading icon. The color swatch ties to the site's MonorailCSS color palette.

- `Title` (`string?`), `Color` (`string`, default `"primary"`), `Icon` (`RenderFragment?`), `ChildContent`.

```razor
<Card Title="Before you begin" Color="primary">
    Confirm .NET 11 is installed and you can run <code>dotnet --version</code>.
</Card>
```

### 4. Use `<CardGrid>` to group multiple cards

Renders a responsive grid (`grid-cols-1 sm:grid-cols-<Columns>`) wrapping its children.

- `Columns` (`string`, default `"2"`), `ChildContent`.

```razor
<CardGrid Columns="3">
    <Card Title="Install"   Color="primary">…</Card>
    <Card Title="Configure" Color="tip">…</Card>
    <Card Title="Deploy"    Color="success">…</Card>
</CardGrid>
```

### 5. Use `<LinkCard>` when the whole card should navigate

Renders an `<a>`-wrapped `<Card>` that highlights on hover.

- `Title` (`string?`), `Href` (`string?`), `Color` (`string`, default `"primary"`), `Icon` (`RenderFragment?`), `ChildContent`.

```razor
<CardGrid Columns="2">
    <LinkCard Title="Getting Started" Href="/tutorials/getting-started/first-site" Color="primary">
        Scaffold a Pennington site from scratch.
    </LinkCard>
    <LinkCard Title="DocSite tutorial" Href="/tutorials/docsite/scaffold" Color="tip">
        Turn a bare site into a structured docs site.
    </LinkCard>
</CardGrid>
```

### 6. Use `<Badge>` for inline status chips

Renders a small pill-shaped span with a variant color and size.

- `Text` (`string`, default `""`).
- `Variant` (`string`, default `"note"`) — `success`, `tip`, `caution`, `danger`, `note`.
- `Size` (`string`, default `"medium"`) — `small`, `medium`, `large`.

```razor
<p>Status: <Badge Text="Stable" Variant="success" Size="small" /></p>
```

---

## Verify

- Run `dotnet run` and open a page that uses the markdown-component flow.
- Confirm the component tags render as real HTML rather than remaining literal tags in the output.
- For edge cases in tag syntax, consult the [Mdazor project](https://github.com/phil-scott-78/Mdazor) rather than duplicating that guidance here.

## Related

- Reference: [Content components](/reference/ui/content) — full parameter tables for each component.
- Reference: [Navigation components](/reference/ui/navigation), [Utility components](/reference/ui/utility).
- External: [Mdazor](https://github.com/phil-scott-78/Mdazor) — underlying component-tag syntax, registration model, nested markdown behavior, and limitations.

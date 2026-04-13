---
title: "Use UI components inside markdown"
description: "Use Pennington.UI components inside markdown through Pennington's Mdazor-based component support, and link to Mdazor for the underlying tag syntax and parser details."
section: "content-authoring"
order: 70
tags: []
uid: how-to.content-authoring.ui-components-in-markdown
isDraft: true
search: false
llms: false
---

> **In this page.** Using Pennington.UI components such as `<Steps>`/`<Step>`, `<Card>`, `<CardGrid>`, `<LinkCard>`, and `<Badge>` inside markdown through Pennington's Mdazor-based component support, plus the Pennington-specific component surface that matters when authoring docs.
>
> **Not in this page.** Authoring your own Razor component (see the tutorial in Beyond the Basics) or re-documenting Mdazor's parser internals, nesting rules, and limitations in full.

> **Scope note.** Pennington's component-tags-in-markdown story is based on [Mdazor](https://github.com/phil-scott-78/Mdazor), a Markdig extension for rendering Razor components inside markdown. This page stays Pennington-specific: which built-in components are useful here, which parameters matter in practice, and where to link out for the deeper syntax and parser behavior.

## When to use this

- You want richer UI blocks in markdown than plain Markdown syntax can provide.
- You are using Pennington's Mdazor-based component support and need the Pennington-facing guidance on which built-in components to reach for.
- You want to link readers to Mdazor for the full tag syntax instead of duplicating that project here.

## Assumptions

- You have an existing Pennington site using either `AddDocSite`, `AddBlogSite`, or a custom `AddPennington` setup.
- The site has the Mdazor-based markdown-component path enabled for the markdown you are authoring.
- You can import `Pennington.UI.Components` wherever those component tags need to resolve.

For the underlying component-tag syntax, nested markdown behavior, unknown-component fallback, and current limitations, link to the [Mdazor README](https://github.com/phil-scott-78/Mdazor).

---

## Steps

### 1. Make the component tags available

- Import `Pennington.UI.Components` where your markdown-component setup expects those tags to resolve.
- Keep this setup local to the site project so the markdown authoring surface stays predictable.
- Link to Mdazor rather than re-explaining how component registration works internally.

```razor
@using Pennington.UI.Components
```

### 2. Use `<Steps>` and `<Step>` for ordered procedures in markdown

- Reach for `<Steps>` when the page needs a more structured procedure than plain numbered markdown lists.
- Keep the markdown body focused on task steps; let the component provide the chrome.
- Parameters (verified against `src/Pennington.UI/Components/Steps.razor` and `Step.razor`):
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

- Use `<Card>` when the content wants a framed panel rather than an alert block.
- Keep the component usage simple in the docs, then point to Mdazor if readers need deeper tag rules.
- Renders a titled rounded card with an optional leading icon. The color swatch ties to the site's MonorailCSS color palette.
- Parameters (verified against `src/Pennington.UI/Components/Card.razor`):
  - `Title` (`string?`), `Color` (`string`, default `"primary"`), `Icon` (`RenderFragment?`), `ChildContent`.

```razor
<Card Title="Before you begin" Color="primary">
    Confirm .NET 11 is installed and you can run <code>dotnet --version</code>.
</Card>
```

### 4. Use `<CardGrid>` to group multiple cards

- Renders a responsive grid (`grid-cols-1 sm:grid-cols-<Columns>`) wrapping its children.
- Parameters (verified against `src/Pennington.UI/Components/CardGrid.razor`):
  - `Columns` (`string`, default `"2"`), `ChildContent`.

```razor
<CardGrid Columns="3">
    <Card Title="Install"        Color="primary">…</Card>
    <Card Title="Configure"      Color="tip">…</Card>
    <Card Title="Deploy"         Color="success">…</Card>
</CardGrid>
```

### 5. Use `<LinkCard>` when the whole card should navigate

- Renders a `<a>`-wrapped `<Card>` that highlights on hover.
- Parameters (verified against `src/Pennington.UI/Components/LinkCard.razor`):
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

- Renders a small pill-shaped span with a variant color and size.
- Parameters (verified against `src/Pennington.UI/Components/Badge.razor`):
  - `Text` (`string`, default `""`).
  - `Variant` (`string`, default `"note"`) — accepted values: `success` (emerald), `tip` (sky), `caution` (amber), `danger` (rose), `note` (base).
  - `Size` (`string`, default `"medium"`) — accepted values: `small`, `medium`, `large`.

```razor
<p>Status: <Badge Text="Stable" Variant="success" Size="small" /></p>
```

---

## Verify

- Run `dotnet run` and open a page that uses the markdown-component flow.
- Confirm the component tags render as real HTML rather than remaining literal tags in the output.
- If a reader needs to understand why a specific tag shape does or does not work, send them to the [Mdazor project](https://github.com/phil-scott-78/Mdazor) rather than duplicating that parser guidance here.

## Related

- Related reference: [Content components](/reference/ui/content) — full parameter tables for each component.
- Related reference: [Navigation components](/reference/ui/navigation), [Utility components](/reference/ui/utility).
- External: [Mdazor](https://github.com/phil-scott-78/Mdazor) — underlying component-tag syntax, registration model, nested markdown behavior, and limitations.

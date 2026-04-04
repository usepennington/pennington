---
title: "Using UI Elements"
description: "Enhance your content with Penn.UI Razor components — badges, cards, steps, navigation, and more"
uid: "penn.getting-started.using-ui-elements"
order: 1030
---

Penn.UI ships a collection of Razor components designed for documentation and content sites. They're not a general-purpose component library — they do a few things well, and those things happen to be exactly what documentation sites need.

If you're using `Penn.DocSite`, these components are already available. Penn.DocSite registers Penn.UI automatically, so there's no separate package to install or configure. Just use them in your markdown and Razor files.

## Available Components

| Component | Purpose |
|-----------|---------|
| `Badge` | Inline status labels |
| `Card` | Content containers with optional icons |
| `CardGrid` | Responsive grid layout for cards |
| `LinkCard` | Clickable card that navigates to a URL |
| `Steps` / `Step` | Numbered step-by-step instructions |
| `CodeBlock` | Programmatic syntax highlighting |
| `BigTable` | Scrollable table wrapper |
| `TableOfContentsNavigation` | Sidebar navigation from content structure |
| `OutlineNavigation` | On-page heading outline |

## Badge

Inline labels for marking status, versions, or categories. Small, unobtrusive, and color-coded.

```razor
<Badge Text="New" Variant="success" />
<Badge Text="Beta" Variant="tip" />
<Badge Text="Deprecated" Variant="danger" />
<Badge Text="Experimental" Variant="caution" />
<Badge Text="Default" />
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Text` | `string` | `""` | The label text |
| `Variant` | `string` | `"note"` | Color variant: `success`, `tip`, `caution`, `danger`, or default |
| `Size` | `string` | `"medium"` | Size: `small`, `medium`, or `large` |

Badges render inline, so you can drop them into paragraphs or headings. They're particularly useful in API reference pages to flag stability levels.

## Card

A content container with an optional title and icon. Use it to call attention to important information, group related concepts, or create visual breaks in long pages.

```razor
<Card Title="Important Note" Color="primary">
    Penn processes markdown at render time, not build time.
    This means your content is always fresh.
</Card>

<Card Title="Warning" Color="accent">
    <Icon>
        <svg><!-- your icon SVG --></svg>
    </Icon>
    This operation cannot be undone.
</Card>
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string?` | `null` | Card heading |
| `Color` | `string` | `"primary"` | Color theme name from your MonorailCSS palette |
| `Icon` | `RenderFragment?` | `null` | Optional icon slot (SVG or any markup) |
| `ChildContent` | `RenderFragment?` | `null` | Card body content |

## CardGrid

Wraps cards (or any content) in a responsive grid. Two columns by default, collapsing to one on small screens.

```razor
<CardGrid Columns="3">
    <Card Title="Fast">Built on ASP.NET's pipeline.</Card>
    <Card Title="Simple">Markdown in, HTML out.</Card>
    <Card Title="Honest">Code samples from real source.</Card>
</CardGrid>
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Columns` | `string` | `"2"` | Number of grid columns at the `sm` breakpoint |
| `ChildContent` | `RenderFragment?` | `null` | Grid items |

## LinkCard

Like `Card`, but the whole thing is a clickable link. Use these for "next steps" sections or feature grids that link to detail pages.

```razor
<CardGrid>
    <LinkCard Title="Getting Started" Href="/getting-started" Color="primary">
        Build your first Penn site in under five minutes.
    </LinkCard>
    <LinkCard Title="API Reference" Href="/api" Color="accent">
        Every type, method, and option documented.
    </LinkCard>
</CardGrid>
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string?` | `null` | Card heading |
| `Href` | `string?` | `null` | Navigation URL |
| `Color` | `string` | `"primary"` | Color theme name |
| `Icon` | `RenderFragment?` | `null` | Optional icon slot |
| `ChildContent` | `RenderFragment?` | `null` | Card body content |

## Steps and Step

Numbered step-by-step instructions. This is the component powering every tutorial on this site, including the one you're reading. Each step gets a numbered indicator on a vertical timeline.

```razor
<Steps>
    <Step StepNumber="1">
        ## Install the Package

        ```bash
        dotnet add package Penn.DocSite --prerelease
        ```
    </Step>
    <Step StepNumber="2">
        ## Configure Program.cs

        Add `AddDocSite` to your service collection.
    </Step>
    <Step StepNumber="3">
        ## Write Content

        Create markdown files in `Content/`.
    </Step>
</Steps>
```

Steps can contain any content — markdown headings, code blocks, images, other components. They're just containers with a number on the side.

### Step Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `StepNumber` | `string` | `"1"` | The step number displayed in the circle |
| `ChildContent` | `RenderFragment?` | `null` | Step content |

## CodeBlock

Programmatic syntax highlighting for when you need to render code from a Razor component rather than from markdown. The `CodeBlock` component uses Penn's highlighting pipeline, so the output matches your markdown code fences.

```razor
<CodeBlock Language="csharp">
    var options = new DocSiteOptions
    {
        SiteTitle = "My Site",
        Description = "Built with Penn",
    };
</CodeBlock>
```

You can also pass code as a parameter:

```razor
<CodeBlock Language="json" Code="@myJsonString" />
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Language` | `string` | `""` | Language identifier for highlighting (`csharp`, `json`, `bash`, etc.) |
| `Code` | `string?` | `null` | Code string to highlight (alternative to child content) |
| `ChildContent` | `RenderFragment?` | `null` | Code as child content |
| `IsInTabGroup` | `bool` | `false` | Omits standalone container classes when part of a tab group |

## BigTable

A simple wrapper that adds horizontal scrolling to wide tables. Wrap any table content that might overflow on small screens.

```razor
<BigTable>

| Column 1 | Column 2 | Column 3 | Column 4 | Column 5 | Column 6 |
|----------|----------|----------|----------|----------|----------|
| data | data | data | data | data | data |

</BigTable>
```

No parameters beyond `ChildContent`. It just adds `overflow-x-scroll` and reasonable text sizing.

## TableOfContentsNavigation

Generates sidebar navigation from your content structure. Penn builds a navigation tree from your markdown files and their front matter, and this component renders it as a hierarchical list with section headers.

```razor
@using Penn.UI.Components.Navigation
@inject NavigationBuilder NavBuilder

<TableOfContentsNavigation
    TableOfContents="@navItems"
    Section="getting-started" />

@code {
    private ImmutableList<NavigationTreeItem>? navItems;

    protected override async Task OnInitializedAsync()
    {
        navItems = await NavBuilder.BuildNavigationTreeAsync();
    }
}
```

If you're using `Penn.DocSite`, this is already wired into the layout. You'd only use it directly if you're building a custom layout.

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `TableOfContents` | `ImmutableList<NavigationTreeItem>?` | `null` | Navigation tree data |
| `Section` | `string?` | `null` | Filter to a specific content section |

The component highlights the current page automatically via `data-current` attributes and Penn's SPA navigation scripts.

## OutlineNavigation

Shows a "On This Page" outline of headings from the current page. The outline is generated client-side from heading elements in the rendered content.

```razor
@using Penn.UI.Components.Navigation

<OutlineNavigation ContentSelector="article" />
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ContentSelector` | `string` | `""` | CSS selector for the content area to scan for headings |
| `Title` | `string` | `"On This Page"` | Heading text above the outline |

The outline highlights the heading nearest the viewport as you scroll — one of those small touches that makes documentation sites feel polished instead of generated.

## Using Components in Markdown

Penn's markdown pipeline supports Razor components directly in your `.md` files. No special syntax needed — just write the component tags:

```markdown
---
title: "My Guide"
---

Here's how to get started:

<Steps>
<Step stepNumber="1">
## First Step

Do the thing.
</Step>
<Step stepNumber="2">
## Second Step

Do the other thing.
</Step>
</Steps>

<Card Title="Tip" Color="primary">
You can mix markdown and components freely.
</Card>
```

Components must be on their own lines (not inline with paragraph text), and the opening/closing tags must not be indented — markdown parsers treat indented HTML as code blocks, which is not what you want.

## Next Steps

- [Connecting to Roslyn](xref:penn.getting-started.connecting-to-roslyn) — embed live code examples from your solution
- [Deploying to GitHub Pages](xref:penn.getting-started.deploying-to-github-pages) — publish your site with automated builds

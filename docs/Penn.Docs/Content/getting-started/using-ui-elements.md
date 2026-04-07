---
title: "Using UI Elements"
description: "Reference for Penn.UI Razor components: badges, cards, steps, code blocks, and navigation"
uid: "penn.getting-started.using-ui-elements"
order: 1030
---

Penn.UI is a focused set of Razor components built for documentation and content sites. If you registered your site with `AddDocSite`, Penn.UI is already available. There is no extra package to install and no additional configuration required. You can use these components in both `.razor` files and `.md` content files immediately.

## Component Overview

| Component | Purpose |
|-----------|---------|
| `Badge` | Inline status or category label |
| `Card` | Bordered content container with optional title and icon |
| `CardGrid` | Responsive grid layout for cards |
| `LinkCard` | Clickable card that navigates to a URL |
| `Steps` / `Step` | Numbered vertical timeline for instructions |
| `CodeBlock` | Syntax-highlighted code from Razor context |
| `BigTable` | Horizontal scroll wrapper for wide tables |
| `TableOfContentsNavigation` | Sidebar navigation tree from content structure |
| `OutlineNavigation` | On-page heading outline with scroll tracking |

## Badge

Renders an inline color-coded label. Use badges to mark versions, stability levels, or categories.

```razor
<Badge Text="Stable" Variant="success" />
<Badge Text="Beta" Variant="tip" />
<Badge Text="Deprecated" Variant="danger" />
<Badge Text="Caution" Variant="caution" />
<Badge Text="Default" />
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Text` | `string` | `""` | The label text displayed inside the badge |
| `Variant` | `string` | `"note"` | Color variant. Accepted values: `success` (emerald), `tip` (sky), `caution` (amber), `danger` (rose), or any other value for the default neutral style |
| `Size` | `string` | `"medium"` | Controls padding and font size. Accepted values: `small`, `medium`, `large` |

Badges render inline, so you can place them next to headings or inside paragraphs. They pair well with API reference pages where you need to flag stability or version requirements at a glance.

```razor
## CreatePipeline <Badge Text="v2.0+" Variant="tip" Size="small" />
```

## Card

A bordered content container with an optional heading and icon. Cards create visual separation and draw attention to callouts, summaries, or grouped information.

```razor
<Card Title="Note" Color="primary">
    Penn processes markdown at request time, not at build time.
    Content changes are reflected immediately during development.
</Card>
```

To add an icon, use the `Icon` render fragment:

```razor
<Card Title="Warning" Color="accent">
    <Icon>
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-5 h-5">
            <path fill-rule="evenodd" d="M8.485 2.495c.673-1.167 2.357-1.167 3.03 0l6.28 10.875c.673 1.167-.168 2.625-1.516 2.625H3.72c-1.347 0-2.189-1.458-1.515-2.625L8.485 2.495z" clip-rule="evenodd" />
        </svg>
    </Icon>
    <ChildContent>
        This operation cannot be undone.
    </ChildContent>
</Card>
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string?` | `null` | Heading text displayed at the top of the card |
| `Color` | `string` | `"primary"` | Color theme name from your MonorailCSS design palette (e.g., `primary`, `accent`, `base`) |
| `Icon` | `RenderFragment?` | `null` | Optional icon content rendered to the left of the card body. Typically an SVG element |
| `ChildContent` | `RenderFragment?` | `null` | The card body content |

The `Color` parameter maps to your site's MonorailCSS color palette. Penn.DocSite defines `primary`, `accent`, and `base` by default. If you have configured custom colors in your `DocSiteOptions`, you can reference those names here.

## CardGrid

Arranges child elements in a responsive grid. The grid collapses to a single column on small viewports and expands to the specified column count at the `sm` breakpoint.

```razor
<CardGrid Columns="3">
    <Card Title="Parse">Markdown to AST via Markdig.</Card>
    <Card Title="Transform">Pipeline extensions enrich the tree.</Card>
    <Card Title="Render">AST to HTML with syntax highlighting.</Card>
</CardGrid>
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Columns` | `string` | `"2"` | Number of grid columns at the `sm` breakpoint and above |
| `ChildContent` | `RenderFragment?` | `null` | Grid items, typically `Card` or `LinkCard` components |

You can nest any content inside `CardGrid`, not just cards. However, the spacing and layout are optimized for card-shaped children.

## LinkCard

A card that wraps its entire surface in a link. The hover state provides visual feedback. Use `LinkCard` for navigation grids, "next steps" sections, or feature overviews that link to detail pages.

```razor
<CardGrid>
    <LinkCard Title="Getting Started" Href="/getting-started" Color="primary">
        Build your first Penn site in five minutes.
    </LinkCard>
    <LinkCard Title="API Reference" Href="/api" Color="accent">
        Types, methods, and configuration options.
    </LinkCard>
</CardGrid>
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string?` | `null` | Card heading text |
| `Href` | `string?` | `null` | URL the card links to |
| `Color` | `string` | `"primary"` | Color theme name from your MonorailCSS palette |
| `Icon` | `RenderFragment?` | `null` | Optional icon content rendered to the left of the card body |
| `ChildContent` | `RenderFragment?` | `null` | Card body content |

`LinkCard` accepts the same `Icon` parameter as `Card`. The icon renders to the left of the title and body, inside the clickable area.

## Steps and Step

A numbered vertical timeline for step-by-step instructions. `Steps` is the outer container that draws the connecting line. `Step` is each individual item, with a numbered circle on the left.

```razor
<Steps>
    <Step StepNumber="1">
        ## Install the package

        ```bash
        dotnet add package Penn.DocSite --prerelease
        ```
    </Step>
    <Step StepNumber="2">
        ## Configure services

        Register Penn in your `Program.cs`:

        ```csharp
        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "My Docs",
        });
        ```
    </Step>
    <Step StepNumber="3">
        ## Add content

        Create markdown files in the `Content/` directory.
    </Step>
</Steps>
```

Each step can contain any content: markdown headings, code blocks, images, nested components. They are containers with a number on the side, nothing more.

### Steps Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Type` | `string` | `"primary"` | Reserved for future styling variants |
| `ChildContent` | `RenderFragment?` | `null` | One or more `Step` components |

### Step Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `StepNumber` | `string` | `"1"` | The number displayed in the circle indicator |
| `ChildContent` | `RenderFragment?` | `null` | The step body content |

The `StepNumber` parameter is a string, not an integer. This means you can use labels like `"A"`, `"B"`, `"C"` if your sequence calls for it, though numeric steps are the common case.

## CodeBlock

Provides syntax highlighting when you need to render code programmatically from a Razor component. `CodeBlock` uses Penn's highlighting pipeline (TextMate-based), so the output matches your markdown fenced code blocks.

There are two ways to provide code. You can pass it as child content:

```razor
<CodeBlock Language="csharp">
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddDocSite(() => new DocSiteOptions
    {
        SiteTitle = "My Site",
    });
</CodeBlock>
```

Or you can pass it as a string parameter, which is useful when the code comes from a variable:

```razor
<CodeBlock Language="json" Code="@configJson" />

@code {
    private string configJson = """
        {
            "title": "My Site",
            "version": "1.0.0"
        }
        """;
}
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Language` | `string` | `""` | **Required.** Language identifier for syntax highlighting (e.g., `csharp`, `json`, `bash`, `xml`, `javascript`) |
| `Code` | `string?` | `null` | Code to highlight, as a string. Provide either this or `ChildContent` |
| `ChildContent` | `RenderFragment?` | `null` | Code to highlight, as child content. Provide either this or `Code` |
| `IsInTabGroup` | `bool` | `false` | When `true`, omits standalone container CSS classes. Set this when the code block is rendered inside a tabbed code group |

`Language` is an editor-required parameter. If you omit it, the component renders an error message instead of highlighted code.

When you use child content, the component automatically normalizes indentation. Leading whitespace that comes from your `.razor` file indentation is stripped so the output is clean.

For most documentation content, markdown fenced code blocks are simpler and preferred. Use `CodeBlock` when you need to render code dynamically in a Razor page or layout, or when embedding highlighted code inside another component.

## BigTable

Wraps content in a horizontally scrollable container with compact text sizing. Use it around markdown tables that have too many columns to fit on small screens.

```razor
<BigTable>

| Method | Route | Auth | Cache | Rate Limit | Description |
|--------|-------|------|-------|-------------|-------------|
| GET | /api/items | Yes | 60s | 100/min | List all items |
| POST | /api/items | Yes | None | 20/min | Create an item |
| DELETE | /api/items/:id | Yes | None | 10/min | Delete an item |

</BigTable>
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Table content (typically a markdown or HTML table) |

`BigTable` has no configuration beyond its child content. It applies `overflow-x-scroll` and `text-sm` to the wrapper, and that is all it does.

## TableOfContentsNavigation

Renders a sidebar navigation tree from your site's content structure. Penn builds a `NavigationTreeItem` hierarchy from your markdown files and their front matter metadata (title, order, section). This component displays that tree as a nested list with section headers and active-page highlighting.

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

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `TableOfContents` | `ImmutableList<NavigationTreeItem>?` | `null` | The navigation tree data built by `NavigationBuilder` |
| `Section` | `string?` | `null` | Optional filter to show only a specific content section |
| `SectionHeaderStructureClass` | `string` | `"font-display font-medium first:pt-0"` | CSS classes for section header layout |
| `SectionHeaderColorClass` | `string` | `"text-base-900 dark:text-base-50"` | CSS classes for section header colors |
| `LinkStructureClass` | `string` | *(see source)* | CSS classes for child link layout |
| `LinkColorClass` | `string` | *(see source)* | CSS classes for child link colors, including active state |
| `RootLinkStructureClass` | `string` | `"block w-full py-1"` | CSS classes for top-level link layout |
| `RootLinkColorClass` | `string` | *(see source)* | CSS classes for top-level link colors |

If you are using `Penn.DocSite`, this component is already wired into the default layout. The layout passes the navigation tree and handles section filtering for you. You only need to use this component directly when building a custom layout.

The component highlights the current page automatically using `data-current` attributes, which Penn's SPA navigation scripts keep in sync during client-side page transitions.

## OutlineNavigation

Displays an "On This Page" outline generated client-side from heading elements in the rendered content. As you scroll, the outline highlights the heading nearest the viewport.

```razor
@using Penn.UI.Components.Navigation

<OutlineNavigation ContentSelector="article" Title="On This Page" />
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ContentSelector` | `string` | `""` | **Required.** CSS selector for the content area to scan for headings (e.g., `"article"`, `"#main-content"`, `".prose"`) |
| `Title` | `string` | `"On This Page"` | Heading text displayed above the outline list |
| `ContainerStructureClass` | `string` | `"border-l border-base-200 dark:border-base-800"` | CSS classes for the outer container layout |
| `ContainerColorClass` | `string` | `""` | CSS classes for the outer container colors |
| `ListStructureClass` | `string` | `"list-none pl-4"` | CSS classes for the heading list layout |
| `ListColorClass` | `string` | `"text-neutral-500 dark:text-neutral-400"` | CSS classes for the heading list colors |
| `OutlineLinkColorClass` | `string` | *(see source)* | CSS classes for individual link colors, including the selected state |
| `OutlineLinkStructureClass` | `string` | *(see source)* | CSS classes for individual link layout |

If you are using `Penn.DocSite`, the outline navigation is already included in the default layout. The layout passes `ContentSelector` pointed at the article element. You only need this component directly when building a custom layout.

The outline is generated entirely client-side using JavaScript. It scans the DOM for heading elements within the `ContentSelector` target, builds the link list, and attaches a scroll listener for active-heading tracking.

## Using Components in Markdown

Penn's markdown pipeline supports Razor components directly in `.md` files. Write the component tags as you would in a `.razor` file.

There are three rules to follow:

1. **Components must be on their own lines.** Do not place a component tag inline within a paragraph. The markdown parser treats inline HTML differently from block-level HTML.

2. **Do not indent component tags.** Markdown parsers treat lines indented by four or more spaces as code blocks. Keep your opening and closing tags flush with the left margin.

3. **Use blank lines around component content.** If you include markdown inside a component (headings, lists, code fences), separate it from the component tags with blank lines so the parser processes it as markdown rather than raw HTML.

Here is a complete example:

```markdown
---
title: "Setup Guide"
---

## Overview

This guide walks through initial setup.

<Badge Text="Required" Variant="caution" />

<Steps>
<Step StepNumber="1">

## Install dependencies

Run the following command:

```bash
dotnet add package Penn.DocSite --prerelease
```

</Step>
<Step StepNumber="2">

## Verify the installation

Build and run your project:

```bash
dotnet run
```

</Step>
</Steps>

<Card Title="Tip" Color="primary">

You can mix markdown and components freely as long as you
follow the three rules above.

</Card>
```

Note that `StepNumber` uses a lowercase `s` for the attribute name in markdown context. Razor component parameters in HTML are case-insensitive, but the convention in Penn's documentation is to use the casing shown in each component's parameter table.

## Next Steps

- [Connecting to Roslyn](xref:penn.getting-started.connecting-to-roslyn) -- embed live code examples verified against your actual solution
- [Deploying to GitHub Pages](xref:penn.getting-started.deploying-to-github-pages) -- publish your site with a GitHub Actions workflow

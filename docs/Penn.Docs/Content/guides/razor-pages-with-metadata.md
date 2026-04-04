---
title: "Razor Pages with Metadata"
description: "Use @page Razor components alongside markdown content in Penn's content pipeline"
uid: "penn.guides.razor-pages-with-metadata"
order: 2060
---

Not everything fits neatly into a markdown file. Sometimes you need a custom Razor component with its own layout, interactive elements, or data-driven content. Penn's <xref:T:Penn.Content.RazorPageContentService> lets you mix `@page` Razor components into the same content pipeline as your markdown pages -- they get discovered, included in static generation, and show up in the search index.

This is useful for pages like "About", "Contact", dashboards, or anything where markdown feels like the wrong tool.

## How RazorPageContentService Works

`RazorPageContentService` scans the assemblies you configure for types that:

1. Inherit from `ComponentBase`
2. Have one or more `[RouteAttribute]` attributes (the compiled form of `@page "/some-path"`)
3. Use **non-parameterized** routes (no `{id}` or `{*slug}` segments)

For each qualifying component, it creates a `ContentRoute` from the route template and yields a `DiscoveredItem` for the content pipeline.

### What It Skips

- **Parameterized routes**: Routes like `@page "/posts/{slug}"` are excluded because the service can't enumerate all possible values. These are typically catch-all routes like `Pages.razor` that render markdown content.
- **Abstract types**: Base components are ignored.
- **Dynamic assemblies**: If an assembly can't be scanned (reflection errors), it's silently skipped.

### Registration

`RazorPageContentService` is registered automatically by `AddPenn()` when you configure `AdditionalRoutingAssemblies`:

```csharp
builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Site";
    penn.AdditionalRoutingAssemblies = [typeof(Program).Assembly];
    // ...
});
```

If you're using `AddDocSite()`, it handles this for you -- it registers the entry assembly plus any assemblies you pass via `DocSiteOptions.AdditionalRoutingAssemblies`.

## Creating a Razor Page

### 1. Create the Component

Create a standard `@page` Razor component:

```razor
@page "/about"

<PageTitle>About Us</PageTitle>

<div class="prose dark:prose-invert">
    <h1>About This Project</h1>
    <p>
        This project exists because someone thought "I should build a content engine"
        and then couldn't stop.
    </p>
</div>
```

Multiple `@page` directives work fine -- each non-parameterized route becomes a separate `DiscoveredItem`:

```razor
@page "/about"
@page "/about-us"
```

### 2. Place It in Your Components Directory

The component goes wherever your Blazor components live. A common convention:

```
Components/
  Pages/
    About.razor
    Contact.razor
```

### 3. It Just Works

With `AdditionalRoutingAssemblies` configured, the `RazorPageContentService` discovers your `@page` components at startup. During static generation, `OutputGenerationService` crawls these pages just like markdown content -- fetching the HTML via HTTP and writing it to the output directory.

## The Pages.razor Catch-All

In a Penn site, you typically have a `Pages.razor` component with a catch-all route that renders markdown content:

```razor
@page "/{*slug}"

@* This catches all routes not handled by specific @page components *@
@* It loads the markdown content for the given slug and renders it *@
```

`RazorPageContentService` ignores this component because its route contains `{*slug}` -- a parameterized segment. The markdown content service handles those routes instead. This is by design: specific `@page` components take precedence (Blazor's routing rules), and the catch-all handles everything else.

## Static Generation

During `dotnet run -- build`, Penn discovers all content sources:

1. **Markdown content services** yield pages from `.md` files
2. **RazorPageContentService** yields pages from `@page` components
3. **OutputGenerationService** crawls every discovered URL via HTTP and writes the HTML to disk

Your Razor pages end up as static HTML files in the output directory, same as markdown pages. No special handling needed.

## Search Integration

Pages discovered by `RazorPageContentService` are included in the content pipeline and can be indexed for search. The `SearchIndexBuilder` processes them like any other `RenderedItem`.

## Practical Example

A documentation site with a custom interactive demo page:

```
Content/
  index.md
  guides/
    getting-started.md
Components/
  Pages/
    InteractiveDemo.razor    <-- @page "/demo"
    About.razor              <-- @page "/about"
```

Both markdown pages and Razor pages appear in the generated site. The sidebar navigation includes markdown pages (from the content structure), and the Razor pages are accessible at their declared routes.

```csharp
builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Project";
    penn.AdditionalRoutingAssemblies = [typeof(Program).Assembly];
    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});
```

The Razor pages at `/demo` and `/about` are discovered automatically. The markdown pages at `/` and `/guides/getting-started` come from the content directory. Everything ends up in the same static output.

## Limitations

- **No metadata sidecar files in v2**: Unlike some documentation systems, Penn v2 doesn't currently use `.yml` sidecar files for Razor page metadata. The page title comes from `<PageTitle>` in the component.
- **Non-parameterized routes only**: `RazorPageContentService` can't discover pages with route parameters. Those pages exist at runtime but aren't included in static generation automatically.
- **Assembly scanning**: The service scans assemblies at startup. If you add a new `@page` component, you need to restart the dev server for it to be discovered. Hot reload updates existing components but doesn't re-scan for new routes.

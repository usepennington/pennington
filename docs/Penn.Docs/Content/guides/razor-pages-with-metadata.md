---
title: "Razor Pages with Metadata"
description: "Mix @page Razor components into Penn's content pipeline alongside markdown"
uid: "penn.guides.razor-pages-with-metadata"
order: 2060
---

Not everything belongs in a markdown file. A custom dashboard, an interactive demo, a page that pulls data from a service at render time -- these all call for a Razor component. Penn's `RazorPageContentService` discovers `@page` Razor components and feeds them into the same content pipeline as markdown pages. They get crawled during static generation, indexed for search, and verified for broken links.

## How RazorPageContentService Works

`RazorPageContentService` scans one or more .NET assemblies at startup. For each type it finds, it checks three conditions:

1. The type inherits from `ComponentBase`.
2. The type has at least one `[RouteAttribute]` (the compiled form of an `@page` directive).
3. The route template contains no parameter segments (no `{` characters).

Types that pass all three checks produce a `DiscoveredItem` for each qualifying route. The service calls `ContentRouteFactory.FromRazorPage(template)` to convert the `@page` template into a `ContentRoute` with a canonical URL and an output file path.

### What gets skipped

**Parameterized routes.** Any route containing `{` is excluded. This covers catch-all routes like `/{*fileName:nonfile}`, typed parameters like `/posts/{id}`, and optional segments. The service has no way to enumerate the possible values, so it leaves these to the components and content services that handle them.

**Abstract types.** Base components that other pages inherit from are ignored.

**Unloadable assemblies.** If an assembly throws during reflection (dynamic assemblies, for instance), the service catches the exception and moves on.

## Configuring Assembly Scanning

`RazorPageContentService` only runs when you give it assemblies to scan.

### With AddPenn

Set `AdditionalRoutingAssemblies` on `PennOptions`:

```csharp
builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Site";
    penn.AdditionalRoutingAssemblies = [typeof(Program).Assembly];
    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});
```

Penn registers a `RazorPageContentService` as an `IContentService` only when this array is non-empty. If you don't set it, no Razor page scanning happens.

### With AddDocSite

`DocSiteOptions` has its own `AdditionalRoutingAssemblies` property. The `AddDocSite()` method automatically includes `Assembly.GetEntryAssembly()` (your application) and merges in anything you specify:

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "Project documentation",
    AdditionalRoutingAssemblies = [typeof(SomeOtherLibrary.Pages.About).Assembly],
});
```

In most cases, you don't need to set `AdditionalRoutingAssemblies` on `DocSiteOptions` at all -- the entry assembly is included automatically, and that's where your `@page` components live.

## Creating a Razor Page

Create a standard Blazor component with an `@page` directive. Place it wherever your project's components live.

```razor
@page "/about/"

<PageTitle>About Us</PageTitle>

<div class="prose dark:prose-invert">
    <h1>About This Project</h1>
    <p>Built with Penn.</p>
</div>
```

Three things to note:

**Use a trailing slash.** Penn normalizes all routes to trailing-slash form. Write `@page "/about/"` rather than `@page "/about"`. The component still works without the trailing slash, but the build will emit a warning (see [Trailing-Slash Warnings](#trailing-slash-warnings) below).

**Set `<PageTitle>`.** During static generation, the `<PageTitle>` component sets the `<title>` element in the rendered HTML. This is how the page gets its title in browser tabs and search results.

**Multiple `@page` directives work.** Each non-parameterized route becomes a separate `DiscoveredItem` in the pipeline:

```razor
@page "/about/"
@page "/about-us/"
```

Both URLs will be crawled and written to the output directory.

## The Catch-All Route

Penn doc sites use a `Pages.razor` component with a catch-all route to render markdown content:

```razor
@page "/{*fileName:nonfile}"
```

This component loads the markdown page matching the URL slug and renders it through the doc site layout. `RazorPageContentService` ignores it because the route contains `{*fileName:nonfile}` -- a parameterized segment.

Blazor's routing gives precedence to specific routes over catch-all routes. When a user navigates to `/about/`, Blazor matches your `About.razor` component directly. When they navigate to `/guides/getting-started/`, no specific `@page` component matches, so the catch-all in `Pages.razor` handles the request and loads the markdown content.

This means your Razor pages and markdown pages coexist without any routing configuration. Specific `@page` directives win, and the catch-all handles everything else.

## Static Generation

During `dotnet run -- build`, `OutputGenerationService` calls `DiscoverAsync()` on every registered `IContentService` to collect pages. `RazorPageContentService` contributes its routes alongside the markdown content service. The generator HTTP-crawls each URL and writes the rendered HTML to the output directory.

Output paths follow Penn's standard convention: `/about/` becomes `about/index.html`, `/tools/demo/` becomes `tools/demo/index.html`. No special configuration is needed -- if assembly scanning finds your `@page` component, static generation picks it up.

## Search Integration

`RazorPageContentService` sets `SearchPriority` to 5. The default for `MarkdownContentService` is 10. Higher values rank higher in search results, so markdown content appears above Razor pages by default.

This makes sense for documentation sites where markdown is the primary content. You can adjust markdown priority through `MarkdownContentServiceOptions` if needed. See <xref:penn.guides.implement-search-functionality> for details on search priority tuning.

`SearchIndexBuilder` processes `RenderedItem` values regardless of which content service produced them. As long as your Razor page renders HTML, it's indexable.

## Trailing-Slash Warnings

Penn enforces trailing slashes on all content URLs. When `RazorPageContentService` discovers a route without a trailing slash, it records the template and component type name. `ContentPipeline.GenerateAsync()` then emits a build warning:

```
WARNING: Razor @page directive "/about" is missing a trailing slash (in MyApp.Components.Pages.About)
```

The page still works -- `ContentRouteFactory.FromRazorPage()` normalizes the URL regardless. Adding the trailing slash to the directive silences the warning:

```razor
@page "/about/"
```

## Practical Example

A documentation site that combines markdown guides with a custom interactive playground page:

```
Content/
  getting-started/
    installation.md
    configuration.md
  guides/
    writing-content.md
Components/
  Pages/
    Playground.razor      @page "/playground/"
    About.razor           @page "/about/"
```

The `Program.cs` wiring:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Library",
    Description = "Documentation for My Library",
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

Since `AddDocSite()` automatically scans the entry assembly, both `Playground.razor` and `About.razor` are discovered. The markdown files are discovered by the markdown content service. Running `dotnet run -- build` produces:

```
getting-started/installation/index.html
getting-started/configuration/index.html
guides/writing-content/index.html
playground/index.html
about/index.html
```

Markdown pages get sidebar navigation from their front matter and directory structure. Both appear in the search index and sitemap.

## Sidecar Metadata

Razor pages can carry front matter metadata through sidecar files. Place a `.razor.metadata.yml` file next to the component file:

```
Components/
  Pages/
    About.razor
    About.razor.metadata.yml
```

The sidecar file is plain YAML using the same `DocFrontMatter` fields available to markdown pages:

```yaml
title: About Us
description: Learn about the project and team
order: 10
section: General
```

### Supported fields

| Field | Type | Description |
|-------|------|-------------|
| `title` | string | Page title for navigation and sitemap |
| `description` | string | Page description for SEO |
| `order` | int | Sort order in navigation (default: `int.MaxValue`) |
| `section` | string | TOC section grouping |
| `isDraft` | bool | Exclude from output when `true` |
| `uid` | string | Cross-reference identifier for `<xref:>` links |
| `tags` | string[] | Tag list |

Pages with a sidecar file appear in the table-of-contents navigation alongside markdown pages. Pages without a sidecar file continue to work as before -- they are crawled and indexed but do not appear in navigation.

### Naming convention

The sidecar file name must match the component name exactly: `{ComponentName}.razor.metadata.yml`. It must be in the same directory as the `.razor` file. Metadata files in other directories are ignored.

### Draft pages

Setting `isDraft: true` in the sidecar file excludes the page from both discovery and navigation. The page will not be crawled during static generation.

```yaml
title: Work in Progress
isDraft: true
```

## Practical example with sidecar metadata

```
Content/
  getting-started/
    installation.md
    configuration.md
Components/
  Pages/
    Playground.razor
    Playground.razor.metadata.yml
    About.razor
    About.razor.metadata.yml
```

`Playground.razor.metadata.yml`:

```yaml
title: Interactive Playground
description: Try out the library features in your browser
order: 50
section: Tools
```

Both Razor pages now appear in the sidebar navigation with the specified titles and ordering. Running `dotnet run -- build` produces:

```
getting-started/installation/index.html
getting-started/configuration/index.html
playground/index.html
about/index.html
```

## Limitations

**Non-parameterized routes only.** `RazorPageContentService` cannot discover pages with route parameters. A component with `@page "/posts/{slug}"` is invisible to the service. If you need parameterized Razor pages in static output, implement a <xref:penn.guides.custom-content-service> that enumerates the possible parameter values and yields a `DiscoveredItem` for each.

**Restart required for new pages.** Assembly scanning happens once at startup. If you add a new `@page` component while the dev server is running, you need to restart it. Hot reload updates the content of existing components but does not re-scan for new route attributes. This also applies to sidecar file changes -- the metadata cache is built once at startup.

For more on extending the content pipeline beyond what `RazorPageContentService` provides, see <xref:penn.guides.custom-content-service> and <xref:penn.under-the-hood.content-processing-pipeline>.

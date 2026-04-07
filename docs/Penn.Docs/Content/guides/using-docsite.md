---
title: "Using the DocSite Package"
description: "Build a documentation site with Penn.DocSite — the batteries-included documentation package"
uid: "penn.guides.using-docsite"
order: 2510
---

Penn.DocSite wraps Penn core, MonorailCSS, and SPA navigation into a single `AddDocSite()` call. It gives you a documentation site with sidebar navigation, search, dark mode, article pagination, and static site generation. The site you are reading was built with it.

## What You Get

- **Sidebar navigation** generated automatically from your content directory structure and front matter
- **SPA navigation** between pages -- clicking a link fetches a JSON envelope and swaps content in place, with no full page reload
- **Search** via FlexSearch, accessible with Ctrl+K
- **Dark/light mode** toggle with a script that prevents flash of unstyled content
- **MonorailCSS** utility-first styling with customizable color schemes and extra styles
- **Static site generation** via `dotnet run -- build` that crawls every page and writes HTML to disk

## Quick Start

### 1. Create a Project

```bash
dotnet new web -n MyDocs
cd MyDocs
dotnet add package Penn.DocSite --prerelease
```

### 2. Write Program.cs

Replace the contents of `Program.cs`:

```csharp
using Penn.DocSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Project Docs",
    Description = "Documentation for my project",
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

Four meaningful lines: register services, build, configure middleware, run.

### 3. Add Content

Create `Content/index.md`:

```markdown
---
title: "Welcome"
description: "Getting started with my project"
order: 1
---

# Welcome

This is the home page. Add more `.md` files and they appear in the sidebar.
```

### 4. Run It

```bash
dotnet watch
```

Navigate to the URL in your terminal. You will see your page rendered in a styled layout with sidebar navigation, search, and a dark mode toggle. Edit `index.md` and save -- the browser refreshes automatically.

> [!TIP]
> Add a `Watch` item to your `.csproj` so `dotnet watch` picks up content changes:
> ```xml
> <ItemGroup>
>     <Watch Include="Content\**\*.*" />
> </ItemGroup>
> ```

For a full walkthrough of project setup, content structure, and static builds, see [Creating Your First Site](xref:penn.getting-started.creating-first-site).

## What AddDocSite Does

The `AddDocSite()` extension method on `IServiceCollection` accepts a factory function that returns a `DocSiteOptions` record. Internally it performs six registrations:

1. **Penn core** via `AddPenn()` -- configures `PennOptions` with your site title, description, canonical URL, and content root path. Registers a markdown content source using `DocSiteFrontMatter` as the front matter type, with the content path from your options and a base page URL of `/`.
2. **MonorailCSS** via `AddMonorailCss()` -- sets up the utility-first CSS framework with your `ColorScheme` (or the default Blue/Purple/Cyan/Pink/Slate scheme) and any `ExtraStyles` you provide.
3. **SPA navigation** via `AddSpaNavigation()` -- registers `SpaPageDataService` and the `/_spa-data/` endpoint that serves JSON envelopes for client-side page transitions.
4. **ComponentRenderer** -- a scoped Blazor `HtmlRenderer` used by island renderers to produce HTML strings from Razor components.
5. **DocSiteArticleSlotRenderer** -- the island renderer for the main article content area, registered as an `IIslandRenderer`.
6. **Razor components** via `AddRazorComponents()` -- DocSite ships its own `App` component, layout, and pages. Your entry assembly is scanned automatically for additional `@page` components.

If you set `ConfigureLocalization` on `DocSiteOptions`, the delegate is forwarded to `PennOptions.Localization` during this registration.

## UseDocSite and RunDocSiteAsync

### UseDocSite

The `UseDocSite()` extension method on `WebApplication` configures the middleware pipeline in the correct order:

```csharp
app.UseAntiforgery();
app.UseStaticFiles();
app.MapRazorComponents<Components.App>()
    .AddAdditionalAssemblies(options.AdditionalRoutingAssemblies);
app.UseMonorailCss();
app.UseSpaNavigation();
app.UsePenn();
```

Calling this single method replaces what would otherwise be six individual middleware registrations. The order matters: static files must be served before Razor components, and MonorailCSS must process responses before they reach the client.

### RunDocSiteAsync

`RunDocSiteAsync()` delegates to Penn's `RunOrBuildAsync()`, which inspects the command-line arguments:

- **No arguments** -- starts the development server.
- **`build` argument** -- starts the server, crawls every known route, writes static HTML and assets to an output directory, generates the search index and SPA data JSON files, then exits.

```bash
# Development
dotnet run

# Static build (root deployment)
dotnet run -- build /

# Static build (subdirectory deployment)
dotnet run -- build /my-project
```

The base path argument tells Penn how to rewrite links and asset paths for subdirectory hosting such as GitHub Pages.

## DocSiteOptions Reference

`DocSiteOptions` is a record with two required properties and a set of optional configuration points:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SiteTitle` | `string` | **required** | Site title shown in the header and browser tab |
| `Description` | `string` | **required** | Site description for SEO meta tags |
| `ColorScheme` | `IColorScheme?` | Blue/Purple/Cyan/Pink/Slate | MonorailCSS color scheme for the site |
| `CanonicalBaseUrl` | `string?` | `null` | Canonical base URL for SEO and feed generation |
| `ContentRootPath` | `FilePath` | `"Content"` | Path to the markdown content directory |
| `HeaderIcon` | `string?` | `null` | HTML string for a custom icon in the header |
| `HeaderContent` | `string?` | `null` | Custom HTML rendered in the site header |
| `FooterContent` | `string?` | `null` | Custom HTML rendered in the site footer |
| `GitHubUrl` | `string?` | `null` | GitHub repository URL displayed in the header |
| `SocialImageUrl` | `string?` | `null` | Default image URL for OpenGraph and Twitter Cards |
| `DisplayFontFamily` | `string?` | `null` | CSS font family for headings |
| `BodyFontFamily` | `string?` | `null` | CSS font family for body text |
| `ExtraStyles` | `string?` | `null` | Additional CSS appended to the generated stylesheet |
| `AdditionalHtmlHeadContent` | `string?` | `null` | Extra HTML injected into `<head>` (font links, meta tags) |
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | Assemblies to scan for `@page` Razor components |
| `SolutionPath` | `string?` | `null` | Path to a `.sln` or `.slnx` file for Roslyn integration (requires `Penn.Roslyn`) |
| `ConfigureLocalization` | `Action<LocalizationOptions>?` | `null` | Delegate to configure locales and default locale |

## DocSiteFrontMatter

`DocSiteFrontMatter` is the metadata model for every markdown page in a DocSite project. It implements `IFrontMatter` plus seven capability interfaces:

| Interface | Property | Type | Purpose |
|-----------|----------|------|---------|
| `IFrontMatter` | `Title` | `string` | Page title, used in navigation and the browser tab |
| `IDescribable` | `Description` | `string?` | Page description for SEO meta tags and search results |
| `IOrderable` | `Order` | `int` | Sort position within a navigation section (lower values first, default `int.MaxValue`) |
| `ICrossReferenceable` | `Uid` | `string?` | Unique identifier for `xref:` cross-reference links |
| `ISectionable` | `Section` | `string?` | Groups pages under a navigation section heading |
| `ITaggable` | `Tags` | `string[]` | Categorization labels |
| `IDraftable` | `IsDraft` | `bool` | When `true`, hides the page from navigation and output |
| `IRedirectable` | `RedirectUrl` | `string?` | Redirects this page's URL to another location |

`DocSiteFrontMatter` does not implement `IDateable`. Documentation pages are not date-oriented content. If you need publication dates, define a custom front matter record or use `Penn.BlogSite`.

A typical front matter block:

```markdown
---
title: "Installation"
description: "How to install and configure the library"
uid: "myproject.installation"
section: "Getting Started"
order: 100
tags: ["setup", "configuration"]
---
```

Penn's pipeline reads these interfaces to decide what features each page supports. For instance, `IOrderable` controls sidebar sort order, `ICrossReferenceable` enables `xref:` link resolution, and `IDraftable` suppresses pages during builds. See [Front Matter Properties](xref:penn.reference.front-matter-properties) for the full reference.

## Customization Examples

### Custom Colors

Use `NamedColorScheme` to pick from the standard Tailwind color palette:

```csharp
using Penn.MonorailCss;

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "Documentation",
    ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorNames.Emerald,
        AccentColorName = ColorNames.Teal,
        TertiaryOneColorName = ColorNames.Sky,
        TertiaryTwoColorName = ColorNames.Violet,
        BaseColorName = ColorNames.Zinc,
    },
});
```

Or generate colors algorithmically from a single hue value (0--360):

```csharp
ColorScheme = new AlgorithmicColorScheme
{
    PrimaryHue = 160,
    BaseColorName = ColorNames.Slate,
}
```

`AlgorithmicColorScheme` derives accent and tertiary hues by default at +180, +90, and -90 degrees from the primary. You can override this with the `ColorSchemeGenerator` property.

For full color scheme documentation, see [Configure Custom Styling](xref:penn.guides.configure-custom-styling).

### Custom Branding

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "Documentation",
    GitHubUrl = "https://github.com/myorg/myproject",
    HeaderContent = """
        <div class="flex items-center gap-2">
            <img src="/logo.svg" alt="Logo" class="h-6 w-6" />
            <span class="font-bold">My Project</span>
        </div>
        """,
    FooterContent = """
        <p class="text-sm text-base-500">Built with Penn.</p>
        """,
});
```

`HeaderContent` and `FooterContent` accept raw HTML. Use MonorailCSS utility classes for layout and styling.

### Custom Fonts

Load a web font via `AdditionalHtmlHeadContent` and apply it with the font family properties:

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "Documentation",
    AdditionalHtmlHeadContent = """
        <link rel="preconnect" href="https://fonts.googleapis.com">
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap"
              rel="stylesheet">
        """,
    DisplayFontFamily = "Inter",
    BodyFontFamily = "Inter",
});
```

`DisplayFontFamily` applies to headings. `BodyFontFamily` applies to body text. Both accept any valid CSS `font-family` value.

### Extra Styles

For CSS that MonorailCSS does not cover, use `ExtraStyles`:

```csharp
ExtraStyles = """
    .custom-banner {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        padding: 2rem;
        border-radius: 0.5rem;
    }
    """
```

The string is appended to the generated MonorailCSS stylesheet.

## Next Steps

- [Creating Your First Site](xref:penn.getting-started.creating-first-site) -- step-by-step tutorial for building a site from scratch
- [Configure Custom Styling](xref:penn.guides.configure-custom-styling) -- full MonorailCSS color scheme and styling reference
- [Adding SPA Navigation](xref:penn.guides.adding-spa-navigation) -- how the island renderer architecture works under the hood
- [Front Matter Properties](xref:penn.reference.front-matter-properties) -- complete reference for all front matter fields and capability interfaces

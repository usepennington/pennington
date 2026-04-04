---
title: "Using the DocSite Package"
description: "Build a documentation site with Penn.DocSite -- the opinionated, inflexible, sufficient-to-the-purpose documentation package"
uid: "penn.guides.using-docsite"
order: 2510
---

Penn.DocSite is the cookie-cutter documentation site package. It wraps Penn core, MonorailCSS, and SPA navigation into a single `AddDocSite()` call and gives you a professional-looking documentation site with search, dark mode, sidebar navigation, and article pagination. You're reading a site built with it right now.

> [!IMPORTANT]
> Penn.DocSite drives the documentation for Penn itself. It is opinionated, inflexible, and sufficient to the purpose. It will change when this site's needs change. Use it for quick documentation sites, proof-of-concepts, or as inspiration for building your own layout with Penn core.

## What You Get

- **Sidebar navigation** built automatically from your content structure
- **Article layout** with previous/next page links
- **SPA navigation** between pages (no full page reloads)
- **Search** with FlexSearch (Ctrl+K)
- **Dark/light mode** toggle
- **MonorailCSS** styling with customizable color schemes
- **Static site generation** via `dotnet run -- build`

## Quick Start

### 1. Create a Project

```bash
dotnet new web -n MyDocs
cd MyDocs
dotnet add package Penn.DocSite
```

### 2. Configure Program.cs

```csharp
using Penn.DocSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(_ => new DocSiteOptions
{
    SiteTitle = "My Project Docs",
    Description = "Documentation for my project",
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

That's the entire `Program.cs`. Four meaningful lines.

### 3. Add Content

Create `Content/index.md`:

```markdown
---
title: "Welcome"
description: "Getting started with my project"
---

# Welcome

This is the home page. Add more `.md` files and they'll appear in the sidebar automatically.
```

### 4. Run It

```bash
dotnet watch
```

Navigate to the URL in your terminal. You'll see your documentation page with a sidebar, search, and dark mode toggle. Edit `index.md` and save -- the browser refreshes automatically.

## What AddDocSite Does

<xref:T:Penn.DocSite.DocSiteServiceExtensions> method `AddDocSite()` orchestrates several registrations:

1. **Penn core** via `AddPenn()` -- configures `PennOptions` with your site title, description, content path, and a markdown content source using `DocSiteFrontMatter`.
2. **MonorailCSS** via `AddMonorailCss()` -- sets up the CSS framework with your color scheme and any extra styles.
3. **SPA navigation** via `AddSpaNavigation()` -- registers `SpaPageDataService` and the data endpoint.
4. **ComponentRenderer** -- scoped Blazor `HtmlRenderer` for island rendering.
5. **DocSiteArticleSlotRenderer** -- the island renderer for the main article content area.
6. **Razor components** via `AddRazorComponents()` -- DocSite ships its own `App` component, layout, and pages.

### UseDocSite

The `UseDocSite()` extension method configures the middleware pipeline:

```csharp
public static WebApplication UseDocSite(this WebApplication app)
{
    app.UseAntiforgery();
    app.UseStaticFiles();
    app.MapRazorComponents<Components.App>()
        .AddAdditionalAssemblies(options.AdditionalRoutingAssemblies);
    app.UseMonorailCss();
    app.UseSpaNavigation();
    app.UsePenn();
    return app;
}
```

### RunDocSiteAsync

`RunDocSiteAsync()` delegates to `RunOrBuildAsync()`, which checks the command line arguments:

- No arguments: runs the dev server
- `build` argument: generates a static site to the output directory

```bash
# Development
dotnet run

# Static build to ./output
dotnet run -- build /

# Static build for subdirectory deployment
dotnet run -- build /my-project
```

## DocSiteOptions

<xref:T:Penn.DocSite.DocSiteOptions> is a record with the following properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SiteTitle` | `string` | required | Title shown in the header and browser tab |
| `Description` | `string` | required | Site description for SEO meta tags |
| `ColorScheme` | `IColorScheme?` | Blue/Purple/Cyan/Pink/Slate | MonorailCSS color scheme |
| `CanonicalBaseUrl` | `string?` | `null` | Canonical URL for SEO and feed generation |
| `ContentRootPath` | `FilePath` | `"Content"` | Path to the content directory |
| `HeaderIcon` | `string?` | `null` | HTML for a custom header icon |
| `HeaderContent` | `string?` | `null` | Custom HTML in the header |
| `FooterContent` | `string?` | `null` | Custom HTML in the footer |
| `GitHubUrl` | `string?` | `null` | GitHub repository URL (shown in header) |
| `SocialImageUrl` | `string?` | `null` | Default social sharing image |
| `DisplayFontFamily` | `string?` | `null` | Font family for headings |
| `BodyFontFamily` | `string?` | `null` | Font family for body text |
| `ExtraStyles` | `string?` | `null` | Additional CSS appended to the stylesheet |
| `AdditionalHtmlHeadContent` | `string?` | `null` | Extra HTML in `<head>` (fonts, meta tags) |
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | Assemblies to scan for @page components |
| `SolutionPath` | `string?` | `null` | Path to .sln/.slnx for Roslyn API docs |

## Customization Examples

### Custom Colors

```csharp
using Penn.MonorailCss;

builder.Services.AddDocSite(_ => new DocSiteOptions
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

Or generate colors algorithmically from a single hue:

```csharp
ColorScheme = new AlgorithmicColorScheme
{
    PrimaryHue = 160, // Green-ish
    BaseColorName = ColorNames.Slate,
}
```

### Custom Branding

```csharp
builder.Services.AddDocSite(_ => new DocSiteOptions
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
    AdditionalHtmlHeadContent = """
        <link rel="preconnect" href="https://fonts.googleapis.com">
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap"
              rel="stylesheet">
        """,
    DisplayFontFamily = "Inter",
    BodyFontFamily = "Inter",
});
```

### File Watching for Development

Add this to your `.csproj` so `dotnet watch` picks up content changes:

```xml
<ItemGroup>
    <Watch Include="Content/**/*.*" />
</ItemGroup>
```

## Content Structure

DocSite uses `DocSiteFrontMatter` for its markdown content. Front matter fields:

```markdown
---
title: "Page Title"
description: "Brief description for SEO and navigation"
uid: "unique-id-for-cross-references"
order: 100
---
```

Pages are organized by directory structure. Create subdirectories for sections:

```
Content/
  index.md
  getting-started/
    installation.md
    configuration.md
  guides/
    first-guide.md
    second-guide.md
```

The sidebar navigation builds automatically from this structure. The `order` property in front matter controls sort order within a section -- lower numbers appear first.

## Static Build

```bash
dotnet run -- build /
```

This generates a complete static site in the `output` directory. Every page is crawled via HTTP, rendered to HTML, and written to disk. Static assets are copied. The search index is generated. SPA data JSON files are created for each page.

For subdirectory deployments (e.g., GitHub Pages), pass the base path:

```bash
dotnet run -- build /my-project
```

See [Deploying to Subdirectories](xref:penn.guides.deploying-to-subdirectories) for CI/CD details.

## This Site Is DocSite

The documentation you're reading right now is built with Penn.DocSite. The source is in `docs/Penn.Docs/` in the Penn repository. It's the canonical example of what DocSite can do -- and also the canonical example of what it can't. If you need something it doesn't support, Penn core gives you the building blocks to do it yourself.

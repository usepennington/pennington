---
title: "Using the DocSite Package"
description: "Learn how to create a documentation site using the DocSite package with customizable branding and styling"
uid: "docs.getting-started.using-docsite"
order: 2510
---

The `MyLittleContentEngine.DocSite` package provides a complete documentation site solution with minimal setup. It includes all the components, layouts, and styling needed to create a professional documentation site with customizable branding.

> [!IMPORTANT]  
> While functional, the `DocSite` package drives the documentation for MyLittleContentEngine. It can and will
> change as this site changes. It is better suited as inspiration or proof-of-concepts than production documentation.

## What You'll Build

You'll create a documentation site with:

- Professional documentation layout with navigation
- API documentation generation
- Search functionality
- Responsive design with dark/light mode
- Custom branding and styling

## Prerequisites

Before starting, ensure you have:

- .NET 9 SDK or later installed
- A code editor (Visual Studio, VS Code, or JetBrains Rider)
- Familiarity with command-line tools

<Steps>
<Step stepNumber="1">
## Create a New Blazor Project

Start by creating a new minimal web project:

```bash
dotnet new web -n MyDocSite
cd MyDocSite
```
</Step>

<Step stepNumber="2">

## Add the DocSite Package

Add the DocSite package reference to your project:

```bash
dotnet add package MyLittleContentEngine.DocSite
```

This package includes all the dependencies you need:
- `MyLittleContentEngine` - Core content management functionality
- `MyLittleContentEngine.UI` - UI components for documentation
- `MyLittleContentEngine.MonorailCss` - CSS framework for styling
- `Mdazor` - Markdown rendering for Blazor
</Step>

<Step stepNumber="3">

## Configure File Watching for Development

Add the following to your `.csproj` file so content changes trigger live reload during development:

```xml
<ItemGroup>
    <Watch Include="Content/**/*.*"/>
</ItemGroup>
```
</Step>

<Step stepNumber="4">

## Configure the DocSite

Replace the content of `Program.cs` with the following minimal configuration:

```csharp
using MyLittleContentEngine.DocSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(_ => new DocSiteOptions
{
    SiteTitle = "My Documentation Site",
    Description = "Documentation for my project",
});

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);
```

This minimal setup provides a complete documentation site with default styling and layout.
</Step>

<Step stepNumber="5">

## Create the Content Structure

Create the content directory structure:

```bash
mkdir -p Content
```

The DocSite package expects your content to be in the `Content` directory by default.
</Step>

<Step stepNumber="6">

## Write Your First Documentation Page

Create your first documentation page at `Content/index.md`:

```markdown
---
title: "Welcome to My Documentation"
description: "Getting started with our documentation site"
---

# Welcome

This is the home page of our documentation site. You can write content using Markdown and it will be automatically rendered with a professional layout.

## Features

- **Responsive Design**: Looks great on all devices
- **Search**: Built-in search functionality
- **API Documentation**: Automatic API reference generation
- **Dark Mode**: Toggle between light and dark themes
```
</Step>

<Step stepNumber="7">

## Customize Your Site

You can customize various aspects of your documentation site by modifying the options in `Program.cs`:

```csharp
builder.Services.AddDocSite(_ => new DocSiteOptions
{
    // Basic site information
    SiteTitle = "My Documentation Site",
    Description = "Comprehensive documentation for my project",
    CanonicalBaseUrl = "https://mydocs.example.com",
    
    // Styling and branding
    PrimaryHue = 235, // Blue theme (0-360)
    BaseColorName = "Zinc", // Base color palette
    GitHubUrl = "https://github.com/myuser/myproject",
    
    // API Documentation (optional)
    SolutionPath = "../../MySolution.slnx",
    IncludeNamespaces = ["MyProject", "MyProject.Core"],
    ExcludeNamespaces = ["MyProject.Tests"],
    
    // Advanced customization
    ExtraStyles = """
        .custom-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
        """
});
```
</Step>

<Step stepNumber="8">

## Add Custom Branding (Optional)

For advanced customization, you can add custom header content or logos:

```csharp
builder.Services.AddDocSite(_ => new DocSiteOptions
{
    SiteTitle = "My Documentation Site",

    // Custom header with logo
    HeaderContent = """
        <div class="flex items-center gap-2">
            <img src="/logo.png" alt="Logo" class="h-8 w-8" />
            <span class="text-xl font-bold">My Docs</span>
        </div>
        """,
    
    // Custom footer
    FooterContent = """
        <div class="text-center text-sm text-base-600 dark:text-base-400">
            © 2024 My Company. All rights reserved.
        </div>
        """,
    
    // Additional HTML for the head section
    AdditionalHtmlHeadContent = """
        <link rel="preconnect" href="https://fonts.googleapis.com">
        <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
        """
});
```
</Step>

<Step stepNumber="9">

## Test Your Documentation Site

Run your site in development mode:

```bash
dotnet watch
```

Navigate to `https://localhost:5001` to see your documentation site in action!

While the page is open, try editing the `Content/index.md` file. You should see the changes reflected immediately without needing to restart the server.
</Step>
</Steps>

## What Success Looks Like

After running `dotnet watch`, navigate to the URL shown in your terminal (typically `http://localhost:5131`).
You'll see:

- Your documentation home page rendered from `Content/index.md`
- A sidebar navigation panel (starts empty but builds automatically as you add pages)
- A clean documentation layout with a dark/light mode toggle

Add a second page at `Content/getting-started.md` with a `title` in the front matter, save it, and watch the
sidebar navigation update automatically — no configuration required.

> [!NOTE]
> You're reading documentation built with DocSite right now. The site you see here is generated from the same
> package, giving you an immediate reference for what's possible.

## Available Configuration Options

The `DocSiteOptions` class provides many customization options:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `SiteTitle` | string | "Documentation Site" | The title displayed in the header |
| `Description` | string | "A documentation site..." | Site description for SEO |
| `PrimaryHue` | int | 235 | Primary color hue (0-360) |
| `BaseColorName` | string | "Zinc" | Base color palette name |
| `GitHubUrl` | string? | null | GitHub repository URL |
| `CanonicalBaseUrl` | string? | null | Canonical URL for SEO |
| `SolutionPath` | string? | null | Path to solution file for API docs |
| `IncludeNamespaces` | string[]? | null | Namespaces to include in API docs |
| `ExcludeNamespaces` | string[]? | null | Namespaces to exclude from API docs |
| `ContentRootPath` | string | "Content" | Path to content directory |
| `ExtraStyles` | string? | null | Additional CSS styles |
| `HeaderIcon` | string? | null | Custom header icon HTML |
| `HeaderContent` | string? | null | Custom header content HTML |
| `FooterContent` | string? | null | Custom footer content HTML |
| `AdditionalHtmlHeadContent` | string? | null | Custom HTML for head section |

## Next Steps

The DocSite package allows you to get up and running quickly, but there are no promises made
that the design or functionality of the site will remain consistent. It's what drives the documentation
for my personal projects, so as my whims change so will the package. Use it for quick proof-of-concepts, demos, or inspiration
for your own documentation site using the `MyLittleContentEngine` services directly.
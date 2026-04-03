---
title: "Razor Pages with Metadata"
description: "Learn how to add metadata to Razor pages using sidecar .yml files for enhanced static site generation with custom titles, descriptions, and ordering."
uid: "docs.guides.razor-pages-with-metadata"
order: 2060
---

MyLittleContentEngine automatically discovers and generates static pages from Razor components in your application. You can enhance these pages with metadata by creating sidecar `.yml` files that provide additional information like titles, descriptions, and ordering for use in sitemaps, RSS feeds, and navigation.

## How It Works

The `RazorPageContentService` automatically scans all assemblies in your application for Razor components that have `@page` directives without parameters. For each component found, it searches for an optional metadata file using the naming convention:

```
ComponentName.razor.metadata.yml
```

For example, if you have an `Index.razor` component, the service will look for `Index.razor.metadata.yml`.

## Metadata File Discovery

The service requires metadata files to be located **in the same directory** as their corresponding Razor component. The discovery process:

1. **Finds the Razor component**: Searches common directories like `Components/Pages`, `Components`, `Pages`, `Views`, `Areas`, `src/Components/Pages`, etc.
2. **Looks for metadata side-by-side**: Once the `.razor` file is found, looks for the `.metadata.yml` file in the exact same directory
3. **Enforces co-location**: Metadata files in different directories are ignored

This approach ensures that components and their metadata are always kept together, making them easier to maintain and organize.

## Creating a Razor Page with Metadata

### Step 1: Create Your Razor Component

Create a standard Razor page component:

```razor
@page "/about"
@page "/about-us"

<PageTitle>About Us</PageTitle>

<h1>About Our Company</h1>

<p>Welcome to our company page...</p>
```

### Step 2: Create the Metadata File

Create a sidecar metadata file named `About.razor.metadata.yml` (matching your component's class name):

```yaml
title: "About Our Company"
description: "Learn more about our company history, mission, and values"
lastMod: "2024-01-15T10:30:00Z"
order: 10
rssItem: true
```

### Step 3: Place Files Side-by-Side

The metadata file **must** be in the same directory as the Razor component:

```
Components/Pages/
├── About.razor
└── About.razor.metadata.yml
```

Other examples of valid side-by-side organization:

```
Pages/
├── Index.razor
├── Index.razor.metadata.yml
├── Services.razor
└── Services.razor.metadata.yml
```

```
src/Components/Pages/
├── Contact.razor
└── Contact.razor.metadata.yml
```

## Metadata Properties

The metadata file supports all properties from the `Metadata` class:

### title
The page title used in RSS feeds and navigation.

```yaml
title: "About Our Company"
```

### description
A brief description of the page content, used in RSS feeds and SEO metadata.

```yaml
description: "Learn about our company history, mission, and values"
```

### lastMod
The last modification date in ISO 8601 format. Used in sitemaps for SEO.

```yaml
lastMod: "2024-01-15T10:30:00Z"
```

### order
Controls the order of pages in navigation and table of contents. Lower numbers appear first.

```yaml
order: 10
```

Default value is `int.MaxValue` (no specific ordering).

### rssItem
Whether this page should be included in RSS feeds.

```yaml
rssItem: true  # Include in RSS (default)
rssItem: false # Exclude from RSS
```

## Complete Example

Here's a complete example with both the Razor component and its metadata:

**Components/Pages/Services.razor**
```razor
@page "/services"
@page "/our-services"

<PageTitle>Our Services</PageTitle>

<h1>Professional Services</h1>

<div class="services-grid">
    <div class="service-card">
        <h2>Web Development</h2>
        <p>Custom web applications built with modern technologies.</p>
    </div>
    
    <div class="service-card">
        <h2>Consulting</h2>
        <p>Expert guidance for your digital transformation.</p>
    </div>
</div>
```

**Services.razor.metadata.yml**
```yaml
title: "Professional Services"
description: "Comprehensive web development and consulting services for modern businesses"
lastMod: "2024-01-20T14:15:00Z"
order: 20
rssItem: true
```

## Static Generation Integration

When MyLittleContentEngine generates your static site:

1. **Page Discovery**: Finds all Razor components with `@page` directives
2. **Metadata Loading**: Searches for and loads corresponding metadata files
3. **Static Generation**: Generates HTML files with enhanced metadata
4. **Sitemap Generation**: Includes `lastMod` dates in `sitemap.xml`
5. **RSS Feed**: Includes pages with `rssItem: true` in RSS feeds
6. **Navigation**: Uses `order` property for consistent page ordering

This feature provides a powerful way to enhance your Razor pages with rich metadata while maintaining the simplicity and flexibility of standard Blazor development.
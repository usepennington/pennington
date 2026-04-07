# P3: JSON-LD Structured Data Generation

## Problem
The site emits Open Graph and Twitter Card meta tags but no JSON-LD structured data. Search engines use JSON-LD for rich results (breadcrumbs, articles, FAQ pages, site navigation).

## Current State
- `MainLayout.razor` emits `og:image`, `og:site_name`, `og:type`, `twitter:image`, `twitter:card` via `<HeadContent>`
- `NavigationBuilder` already computes breadcrumbs and hierarchical navigation
- `IDescribable` and `IDateable` capability interfaces provide description and date metadata
- `SitemapService` generates sitemap.xml with route URLs
- `DocSiteOptions` has `CanonicalBaseUrl` and `SiteTitle`
- No JSON-LD output exists anywhere

## Requirements
- Generate a `<script type="application/ld+json">` block in the page head with structured data appropriate to the page type
- **Article** schema for content pages: `@type: Article`, `headline` (from title), `description`, `datePublished` (from `IDateable`), `author` (from front matter if available)
- **BreadcrumbList** schema for all pages with breadcrumb navigation: derive from the navigation tree already computed by `NavigationBuilder`
- **WebSite** schema on the homepage: `name`, `url`, `description`
- All URLs in JSON-LD must be absolute (use `CanonicalBaseUrl` from `DocSiteOptions` or `PennOptions`)
- Implement as a Razor component (e.g., `StructuredData.razor`) that can be included in layouts, receiving the current page's metadata and breadcrumb path
- If `CanonicalBaseUrl` is not configured, skip JSON-LD generation (relative URLs are invalid in structured data)
- Serialize with `System.Text.Json` — ensure proper escaping of content within `<script>` tags

## Key Files
- `src/Penn.UI/Components/` or `src/Penn.DocSite/Components/` — new `StructuredData` component
- `src/Penn.DocSite/Components/Layout/MainLayout.razor` — include component in `<HeadContent>`
- `src/Penn/Navigation/NavigationBuilder.cs` — breadcrumb data source
- `src/Penn/FrontMatter/Capabilities.cs` — `IDescribable`, `IDateable` for article metadata

---
title: "Using the BlogSite Package"
description: "Build a blog with Penn.BlogSite — posts, tags, archives, series, and RSS"
uid: "penn.guides.using-blogsite"
order: 2500
---

Penn.BlogSite is the blog counterpart to [Penn.DocSite](xref:penn.guides.using-docsite). A single `AddBlogSite()` call wires Penn core, MonorailCSS, and Razor components into a complete blog with posts, tags, archives, series navigation, RSS, and social-media metadata. If DocSite is the documentation cookie cutter, BlogSite is the blog one.

## What BlogSite Gives You

Out of the box, Penn.BlogSite provides:

- **Home page** with optional hero banner, recent-posts feed (most recent 10), project cards sidebar, and social-icon links
- **Blog post page** with rendered markdown, series navigation box (previous/next in series), tag pills, repository link, and OpenGraph + Twitter Card meta tags
- **Archive page** at `/archive` listing every post, date-descending
- **Tags index** at `/tags` showing every tag with its post count
- **Tag filter page** at `/tags/{name}` listing posts for a single tag
- **Dark/light mode** toggle with an inline script that prevents flash of unstyled content
- **RSS feed** and **sitemap** (both toggleable)
- **MonorailCSS** utility-first styling with a customizable color scheme
- **Search** button in the header (Ctrl+K)
- **Mobile-responsive** header with hamburger menu for navigation links

Penn.BlogSite depends on Penn, Penn.UI, and Penn.MonorailCss. You do not need to reference those packages separately. The package handles all service registration, middleware ordering, and Razor component wiring -- you configure options and write markdown.

## Quick Start

### 1. Create a Project

```bash
dotnet new web -n MyBlog
cd MyBlog
dotnet add package Penn.BlogSite
```

### 2. Configure Program.cs

```csharp
using Penn.BlogSite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "My Blog",
    Description = "Thoughts on code and things",
    AuthorName = "Your Name",
    CanonicalBaseUrl = "https://myblog.example.com",
});

var app = builder.Build();
app.UseBlogSite();
await app.RunBlogSiteAsync(args);
```

That is the entire `Program.cs`. Five meaningful lines.

### 3. Add a Post

Create `Content/Blog/hello-world.md`:

```markdown
---
title: "Hello World"
description: "My first blog post"
date: 2026-01-15
tags: [general]
---

Welcome to my blog.
```

Run with `dotnet run` and open `https://localhost:5001`. Your post appears on the home page and at `/blog/hello-world`.

### What Each Call Does

**`AddBlogSite`** registers `BlogSiteOptions` as a singleton, then calls `AddPenn` internally to set `SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`, and `ContentRootPath`. It registers a markdown content source using `AddMarkdownContent<BlogSiteFrontMatter>` with the `BlogContentPath` and `BlogBaseUrl` from your options. It also registers MonorailCSS, Razor components, and a `BlogContentResolver` managed by `AddFileWatched` so the resolver is automatically recreated when files change on disk.

**`UseBlogSite`** calls `UseAntiforgery`, `UseStaticFiles`, `MapRazorComponents<App>` (including any additional assemblies you specified), `UseMonorailCss`, and `UsePenn`.

**`RunBlogSiteAsync`** delegates to `RunOrBuildAsync` -- serve in development, or static-build when you pass `-- build /` on the command line.

## BlogSiteOptions

All configuration lives on the `BlogSiteOptions` record. Here is every property, grouped by purpose.

### Site Identity

| Property | Type | Default | Description |
|---|---|---|---|
| `SiteTitle` | `string` (required) | -- | Displayed in the header, footer, and page titles |
| `Description` | `string` (required) | -- | Site-level meta description |
| `CanonicalBaseUrl` | `string?` | `null` | Base URL for OpenGraph `og:url` tags and RSS links |
| `AuthorName` | `string?` | `null` | Shown in the footer copyright line |
| `AuthorBio` | `string?` | `null` | Reserved for sidebar author bio display |

### Content Paths

| Property | Type | Default | Description |
|---|---|---|---|
| `ContentRootPath` | `string` | `"Content"` | Root directory for all content |
| `BlogContentPath` | `string` | `"Blog"` | Subdirectory under `ContentRootPath` for blog posts |
| `BlogBaseUrl` | `string` | `"/blog"` | URL prefix for blog post routes |
| `TagsPageUrl` | `string` | `"/tags"` | URL for the tags index and tag filter pages |

A post at `Content/Blog/my-post.md` resolves to `/blog/my-post` by default. Change `BlogBaseUrl` to `"/articles"` and it becomes `/articles/my-post`.

### Styling

| Property | Type | Default | Description |
|---|---|---|---|
| `ColorScheme` | `IColorScheme?` | MonorailCSS default | Color palette for the site |
| `ExtraStyles` | `string?` | `null` | Additional CSS appended to the generated stylesheet |
| `DisplayFontFamily` | `string?` | `null` | Font family for headings (`font-display`) |
| `BodyFontFamily` | `string?` | `null` | Font family for body text (`font-sans`) |
| `AdditionalHtmlHeadContent` | `string?` | `null` | Raw HTML injected into `<head>` (font links, analytics, etc.) |

### Home Page Extras

**`HeroContent`** accepts a `HeroContent(Title, Description)` record. The description supports HTML, rendered via `MarkupString`. When set, the hero appears at the top of the home page above the post feed.

**`MyWork`** is an array of `Project(Title, Description, Url)` records. These appear in a sticky sidebar on the home page with title, description, and link for each project.

**`Socials`** is an array of `SocialLink(RenderFragment Icon, string Url)`. The package ships four built-in icons on the `SocialIcons` class: `GithubIcon`, `LinkedInIcon`, `BlueskyIcon`, and `MastodonIcon`. Social links appear below the projects sidebar.

**`MainSiteLinks`** is an array of `HeaderLink(Title, Url)`. These appear in the header navigation, the footer, and the mobile hamburger menu.

### Feature Toggles

| Property | Type | Default | Description |
|---|---|---|---|
| `EnableRss` | `bool` | `true` | Include RSS feed link in `<head>` |
| `EnableSitemap` | `bool` | `true` | Generate a sitemap |

### Advanced

| Property | Type | Default | Description |
|---|---|---|---|
| `AdditionalRoutingAssemblies` | `Assembly[]` | `[]` | Extra assemblies for Razor component routing |
| `SocialMediaImageUrlFactory` | `Func<BlogPostPage, string>?` | `null` | Generates an OG/Twitter image URL per post |

Here is a fully-configured example:

```csharp
builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "Coding with Phil",
    Description = "Posts about .NET, architecture, and open source",
    CanonicalBaseUrl = "https://phil.example.com",
    AuthorName = "Phil",
    HeroContent = new HeroContent(
        "Hi, I'm Phil",
        "I write about <strong>.NET</strong>, open source, and software architecture."),
    MyWork =
    [
        new Project("Penn", "Content engine for .NET", "https://github.com/example/penn"),
        new Project("MonorailCSS", "Utility-first CSS for Blazor", "https://monorailcss.com"),
    ],
    Socials =
    [
        new SocialLink(SocialIcons.GithubIcon, "https://github.com/example"),
        new SocialLink(SocialIcons.BlueskyIcon, "https://bsky.app/profile/example"),
        new SocialLink(SocialIcons.MastodonIcon, "https://mastodon.social/@example"),
    ],
    MainSiteLinks =
    [
        new HeaderLink("Blog", "/"),
        new HeaderLink("Archive", "/archive"),
        new HeaderLink("Tags", "/tags"),
    ],
    SocialMediaImageUrlFactory = post =>
        $"/images/social/{post.Url.TrimStart('/').Replace("/", "-")}.png",
});
```

## Writing Blog Posts

Blog posts are markdown files in the `BlogContentPath` directory. Each file uses `BlogSiteFrontMatter` for its YAML front matter.

### Front Matter Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | `string` | Yes | Post title (defaults to `"Empty title"` if omitted) |
| `description` | `string?` | No | Summary shown in post cards and meta tags |
| `author` | `string` | No | Post author |
| `date` | `DateTime?` | No | Publication date; controls sort order |
| `tags` | `string[]` | No | Tag labels; each generates a tag page link |
| `series` | `string` | No | Series name; groups related posts |
| `isDraft` | `bool` | No | When `true`, the post is excluded from all listings |
| `repository` | `string` | No | URL to source code; shown as a link below the post |
| `uid` | `string?` | No | Cross-reference identifier |
| `redirectUrl` | `string?` | No | Redirect target URL |
| `section` | `string?` | No | Content section grouping |

`BlogSiteFrontMatter` implements `IFrontMatter`, `IDraftable`, `ITaggable`, `IDescribable`, `IDateable`, `ICrossReferenceable`, `ISectionable`, and `IRedirectable`.

### Example Post

```markdown
---
title: "Deploying to Azure Static Web Apps"
description: "Step-by-step guide for deploying a Penn blog to Azure"
author: "Phil"
date: 2026-03-20
tags: [azure, deployment, static-sites]
series: "Deployment Guide"
repository: "https://github.com/example/deploy-sample"
---

Azure Static Web Apps provides free hosting for static sites.

## Prerequisites

- An Azure account
- The Azure CLI installed

## Steps

1. Build your site with `dotnet run -- build /`
2. ...
```

### How Posts Are Discovered

`BlogContentResolver` iterates all registered `IContentService` instances, filters to `MarkdownFileSource` items, and parses each file's front matter as `BlogSiteFrontMatter`. Posts with `isDraft: true` are skipped. The resolver builds a `BlogTag` array for each post with URLs derived from `TagsPageUrl` (e.g., `/tags/azure`). Results are cached with `AsyncLazy` and the cache is invalidated automatically by `FileWatchDependencyFactory` when files change on disk.

All post listings are sorted by date descending.

## Series Support

The `series` front matter field groups related posts. When a post has a non-empty `series` value, the blog post page queries all posts with the same series name and displays them in date-ascending order.

Two UI elements appear on series posts:

1. **Series box** above the post content -- lists every post in the series with the current post highlighted in bold. Other entries link to their respective posts.
2. **Next-in-series prompt** below the post content -- if there is a subsequent post in the series, a card links to it with "Ready for the next article in the series?"

To create a series, give two or more posts the same `series` value:

```yaml
# Post 1: Content/Blog/deploy-part-1.md
---
title: "Deployment Guide: Build"
date: 2026-03-20
series: "Deployment Guide"
---
```

```yaml
# Post 2: Content/Blog/deploy-part-2.md
---
title: "Deployment Guide: Publish"
date: 2026-03-21
series: "Deployment Guide"
---
```

The series box on each post lists both entries. Post 1 shows a next-in-series link to Post 2.

## Tags and Archive

Tags connect your posts into browsable categories. BlogSite provides three tag-related views out of the box, all powered by `BlogContentResolver`.

### Tag Pills

Each blog post displays its tags as rounded pill-shaped links below the post content. Clicking a tag navigates to `/tags/{encoded-tag-name}`.

### Tags Index

The `/tags` page lists every tag used across all posts, ordered by post count descending. Each entry shows the tag name and count, linking to the tag filter page. The data comes from `BlogContentResolver.GetTagsWithCountsAsync()`.

### Tag Filter

The `/tags/{name}` page shows all posts matching the selected tag. Tag names are URL-encoded via `HttpUtility.UrlEncode`, so a tag like "static sites" becomes `/tags/static+sites`. The data comes from `BlogContentResolver.GetPostsByTagAsync()`.

### Archive

The `/archive` page displays every published post using the `BlogSummary` component -- the same article card layout used on the home page feed, but showing all posts instead of the most recent 10. The home page includes a "View all N posts" link to the archive when you have more than five posts.

## Social and SEO Features

The blog post page emits OpenGraph and Twitter Card meta tags automatically:

- `og:title`, `og:description`, `og:url`, `og:type` (`article`), `og:site_name`
- `article:published_time` (when the post has a date)
- `twitter:card` (`summary_large_image`), `twitter:title`, `twitter:description`

### Social Images

If you set `SocialMediaImageUrlFactory`, the factory receives a `BlogPostPage` and returns an image URL string. That URL populates `og:image` and `twitter:image`. When the returned URL starts with `/`, it is automatically prefixed with `CanonicalBaseUrl`:

```csharp
SocialMediaImageUrlFactory = post =>
    $"/images/og/{post.FrontMatter.Title.ToLowerInvariant().Replace(" ", "-")}.png",
```

A post titled "Hello World" would produce `https://myblog.example.com/images/og/hello-world.png` (assuming `CanonicalBaseUrl` is set).

### RSS

When `EnableRss` is `true` (the default), an RSS feed link is included in the HTML `<head>`:

```html
<link type="application/rss+xml" rel="alternate" title="My Blog" href="/rss.xml" />
```

## Customizing the Layout

BlogSite's layout is a hierarchy of Razor components:

- **`App.razor`** -- HTML shell with dark-mode script, RSS link, versioned asset references, and `AdditionalHtmlHeadContent`
- **`MainLayout`** -- header (site title, navigation links, search button, mobile menu, dark/light toggle) and footer (navigation links, copyright)
- **`ContentLayout`** -- centered content wrapper for the home page
- **`ContentWithProseLayout`** -- inherits `ContentLayout` and adds Tailwind prose styling; used by Tags, Tag, and Archive pages

Blog posts use the `MainLayout` directly with a dedicated `BlogPost` component that handles the article header (title and formatted date), series navigation box, rendered HTML body, next-in-series prompt, tag pills, repository link, and a "Back home" navigation link.

To add custom pages from your consuming project, pass your project's assembly via `AdditionalRoutingAssemblies`:

```csharp
builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "My Blog",
    Description = "My blog",
    AdditionalRoutingAssemblies = [typeof(Program).Assembly],
});
```

Razor pages in your project that use `@page` directives will be picked up by BlogSite's router. This is useful for adding an About page, a contact form, or any other custom page alongside the blog.

## Building and Deploying

```bash
# Development with hot reload
dotnet watch --project MyBlog

# Static site build
dotnet run --project MyBlog -- build /
```

`RunBlogSiteAsync` delegates to `RunOrBuildAsync`, the same mechanism used by [DocSite](xref:penn.guides.using-docsite). In development mode it starts the Kestrel server. When `-- build /` is passed, it crawls all routes and writes static HTML to the output directory.

The static output can be deployed to any static host: GitHub Pages, Azure Static Web Apps, Netlify, Cloudflare Pages, or a plain file server. The generated files include your rendered HTML pages, the MonorailCSS stylesheet, RSS feed (if enabled), and sitemap.

## Next Steps

- [Creating Your First Site](xref:penn.getting-started.creating-first-site) -- if you skipped the fundamentals
- [Using the DocSite Package](xref:penn.guides.using-docsite) -- the documentation-site counterpart
- [Multiple Content Sources](xref:penn.guides.multiple-content-sources) -- combine blog posts with other content types in one site

---
title: "BlogSite Quick Start"
description: "Launch a blog with posts, tags, series, archives, and RSS using the Pennington.BlogSite package"
uid: "penn.tutorials.blogsite-quick-start"
order: 30
---

## Beat 1: Create the project and install Pennington.BlogSite

The reader scaffolds an empty ASP.NET project and adds the BlogSite package. The goal is to show that BlogSite is a single-package solution for blogs, parallel to DocSite for documentation.

### What to show
- Terminal commands: `dotnet new web -n AlexBlog`, then `dotnet add package Pennington.BlogSite`
- Brief explanation: `Pennington.BlogSite` bundles Pennington core, MonorailCSS, and a blog-specific layout with home page, post pages, tag pages, and archive
- Show the `.csproj` with the single PackageReference

### Key points
- Like DocSite, BlogSite is a convenience layer -- it internally registers Pennington, MonorailCSS, and its own Razor components
- BlogSite provides: home page with hero and project showcase, blog post listing, individual post pages, tag pages, date-based ordering, RSS feed, and sitemap
- No layout component needs to be created by the reader

## Beat 2: Write Program.cs with BlogSiteOptions

The reader writes ~10 lines of Program.cs configuring the blog's identity and author info. The goal is to get a running blog with the minimum viable options.

### What to show
- Complete Program.cs (~10 lines) using:
  - `M:Pennington.BlogSite.BlogSiteServiceExtensions.AddBlogSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.BlogSite.BlogSiteOptions})` -- takes a factory function returning `BlogSiteOptions`
  - `M:Pennington.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)` -- configures middleware
  - `M:Pennington.BlogSite.BlogSiteServiceExtensions.RunBlogSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` -- delegates to `RunOrBuildAsync`
- The `BlogSiteOptions` record configured with:
  - `P:Pennington.BlogSite.BlogSiteOptions.SiteTitle` -- `"Alex's Dev Blog"` (required)
  - `P:Pennington.BlogSite.BlogSiteOptions.Description` -- `"Notes on .NET, tools, and developer life"` (required)
  - `P:Pennington.BlogSite.BlogSiteOptions.AuthorName` -- `"Alex Chen"`
  - `P:Pennington.BlogSite.BlogSiteOptions.AuthorBio` -- `"Software engineer who writes about .NET, developer tools, and the occasional hot take."`
- Code reference: `T:Pennington.BlogSite.BlogSiteOptions`

### Key points
- `AddBlogSite` is a single package that bundles Pennington, MonorailCSS, and blog components
- The home page will look sparse with just these options -- we'll add the hero, social links, and project showcase after writing the first posts.

## Beat 3: Write the first blog post

The reader creates the first markdown file with `BlogSiteFrontMatter` including date, author, tags, and series. The goal is to introduce the blog-specific front matter fields.

### What to show
- Create the `Content/Blog/` directory
- Create `Content/Blog/building-a-cli-part-1.md` with front matter:
  - `title: "Building a CLI Tool, Part 1: Parsing Arguments"`
  - `date: 2026-03-15`
  - `author: "Alex Chen"`
  - `tags: ["dotnet", "cli"]`
  - `series: "Building a CLI"`
  - `description: "Start a CLI tool from scratch with System.CommandLine"`
- Body: ~150 words with a C# code block showing argument parsing
- Code reference for the front matter type: `T:Pennington.BlogSite.BlogSiteFrontMatter`

### Key points
- `BlogSiteFrontMatter` implements: `T:Pennington.FrontMatter.IFrontMatter`, `T:Pennington.FrontMatter.IDraftable`, `T:Pennington.FrontMatter.ITaggable`, `T:Pennington.FrontMatter.IDescribable`, `T:Pennington.FrontMatter.IDateable`, `T:Pennington.FrontMatter.ICrossReferenceable`, `T:Pennington.FrontMatter.ISectionable`, `T:Pennington.FrontMatter.IRedirectable`
- Key fields unique to blog posts:
  - `P:Pennington.BlogSite.BlogSiteFrontMatter.Date` -- a `DateTime?` used for ordering (newest first) and display
  - `P:Pennington.BlogSite.BlogSiteFrontMatter.Author` -- displayed on the post page
  - `P:Pennington.BlogSite.BlogSiteFrontMatter.Series` -- a string that groups posts into a series (more on this in Beat 4)
  - `P:Pennington.BlogSite.BlogSiteFrontMatter.Repository` -- optional link to a code repository for the post
  - `P:Pennington.BlogSite.BlogSiteFrontMatter.Tags` -- string array for categorization; each tag generates a tag page at `{TagsPageUrl}/{encoded-tag-name}`
  - `P:Pennington.BlogSite.BlogSiteFrontMatter.IsDraft` -- set to `true` to exclude a post from the published site
- Contrast with the core `T:Pennington.FrontMatter.BlogFrontMatter` which is the base version without `RedirectUrl`, `Section`, or `Repository` -- `BlogSiteFrontMatter` is the full-featured variant used by the BlogSite package
- The file path `Content/Blog/building-a-cli-part-1.md` maps to URL `/blog/building-a-cli-part-1/` because of `BlogBaseUrl` being `/blog`

## Beat 4: Write the second post and demonstrate series linking

The reader creates a second post with the same `series` value and sees how BlogSite links them together. The goal is to show series support as a built-in feature.

### What to show
- Create `Content/Blog/building-a-cli-part-2.md` with front matter:
  - `title: "Building a CLI Tool, Part 2: Output Formatting"`
  - `date: 2026-03-22`
  - `author: "Alex Chen"`
  - `tags: ["dotnet", "cli"]`
  - `series: "Building a CLI"`
  - `description: "Add color, tables, and progress bars to your CLI"`
- Body: ~150 words with a code block showing Spectre.Console usage
- Run the site with `dotnet watch`
- Navigate to Part 1 -- show the series navigation that appears, linking to Part 2
- Navigate to Part 2 -- show the series navigation linking back to Part 1
- The series mechanism: `T:Pennington.BlogSite.Services.BlogContentResolver` queries all posts via `M:Pennington.BlogSite.Services.BlogContentResolver.GetAllPostsAsync` and groups by the `Series` property on `BlogSiteFrontMatter`

### Key points
- Series linking is automatic -- any two or more posts with the same `series` string value are grouped and linked chronologically by date
- `T:Pennington.BlogSite.Services.BlogContentResolver` is the service responsible for resolving blog posts, computing tags, and grouping series
- `BlogContentResolver` is registered as a file-watched service (`AddFileWatched<BlogContentResolver>`) so it re-scans content when files change
- `T:Pennington.BlogSite.BlogPostPage` is the resolved post model: it has `FrontMatter` (the `BlogSiteFrontMatter`), `Url`, and `Tags` (array of `T:Pennington.BlogSite.BlogTag`)
- Posts within a series are ordered by `Date` -- the series navigation shows them in chronological order regardless of file name

## Beat 5: Write a standalone post with different tags

The reader creates a third post without a series, using different tags. The goal is to show tag overlap and standalone posts alongside series posts.

### What to show
- Create `Content/Blog/why-i-switched-to-linux.md` with front matter:
  - `title: "Why I Switched to Linux for .NET Development"`
  - `date: 2026-04-01`
  - `author: "Alex Chen"`
  - `tags: ["linux", "workflow", "dotnet"]`
  - `description: "My experience moving from Windows to Fedora"`
- Body: ~150 words of prose (no code blocks), with a `[!TIP]` alert about WSL as a middle ground
- No `series` field -- this is a standalone post

### Key points
- The `dotnet` tag now appears in all three posts, `cli` in two, `linux` and `workflow` in one -- this creates interesting tag page content
- Posts without a `series` value render without series navigation
- The `[!TIP]` alert demonstrates that Pennington's markdown alert extension works in blog posts just as in documentation pages
- Tags are URL-encoded for the tag page URL: `BlogTag` (`T:Pennington.BlogSite.BlogTag`) has `Name` (display) and `Url` (e.g., `/tags/dotnet`)

## Beat 6: Personalize the home page

The reader adds the hero section, social links, and project showcase to BlogSiteOptions. The goal is to show how the home page comes alive with these additions.

### What to show
- Return to `Program.cs` and add to `BlogSiteOptions`:
  - `P:Pennington.BlogSite.BlogSiteOptions.HeroContent` -- `new HeroContent("Hi, I'm Alex", "I write about .NET, developer tools, and making software that doesn't make you cry.")`
  - `P:Pennington.BlogSite.BlogSiteOptions.Socials` -- array of `T:Pennington.BlogSite.SocialLink` with GitHub and Mastodon entries (each takes an SVG `RenderFragment` and a URL)
  - `P:Pennington.BlogSite.BlogSiteOptions.MyWork` -- array of `T:Pennington.BlogSite.Project`: `new Project("Tempo", "Task scheduling for .NET", "https://github.com/...")`
  - `P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl` -- `"https://alexchen.dev"` (needed for RSS and sitemap absolute URLs)
- Code reference: `T:Pennington.BlogSite.HeroContent`, `T:Pennington.BlogSite.SocialLink`, `T:Pennington.BlogSite.Project`
- Run `dotnet watch` and see the home page transform: hero section at the top, social links, and project cards

### Key points
- `HeroContent` has `Title` and `Description` properties displayed prominently on the home page
- `SocialLink` takes a `RenderFragment` for the icon (inline SVG) and a URL string
- `Project` is a simple record with `Title`, `Description`, and `Url` -- these render in the "My Work" section
- `CanonicalBaseUrl` enables absolute URLs in RSS feeds and sitemaps -- without it, feed URLs would be relative

## Beat 7: Run the site and explore all features

The reader starts the dev server and walks through the blog's pages and feeds. The goal is to show every automatic feature BlogSite provides.

### What to show
- Terminal command: `dotnet watch`
- Walk through each feature in the browser:
  1. **Home page** -- hero section with title and description from `HeroContent`, recent posts listed with titles/dates/descriptions, "My Work" section with project cards, social links in the footer/header
  2. **Blog post page** -- navigate to any post, see the title, date, author, tags (as clickable links), and rendered markdown content. For the series posts, see the series navigation at the bottom
  3. **Tag pages** -- click a tag (e.g., "dotnet") to see the tag page at `/tags/dotnet` listing all posts with that tag. Navigate to `/tags/` to see all tags with post counts (via `M:Pennington.BlogSite.Services.BlogContentResolver.GetTagsWithCountsAsync`)
  4. **Date ordering** -- posts appear newest first (the Linux post from April 1, then CLI Part 2 from March 22, then Part 1 from March 15)
  5. **RSS feed** -- note that `P:Pennington.BlogSite.BlogSiteOptions.EnableRss` (default `true`) adds a `<link type="application/rss+xml">` tag pointing to `/rss.xml` in the HTML head. RSS feed serving is not yet implemented in Pennington core -- the `T:Pennington.Feeds.RssFeedBuilder` is registered but no endpoint serves the XML. This is tracked as a known gap
  6. **Sitemap** -- visit `/sitemap.xml` to see the auto-generated sitemap
  7. **Styles** -- note the MonorailCSS styling, dark mode toggle, syntax highlighting in code blocks

### Key points
- The blog home page layout, post layout, tag pages, and archive pages are all provided by BlogSite's Razor components in `src/Pennington.BlogSite/Components/`
- `BlogContentResolver` powers most of the data:
  - `M:Pennington.BlogSite.Services.BlogContentResolver.GetAllPostsAsync` -- all posts ordered by date descending
  - `M:Pennington.BlogSite.Services.BlogContentResolver.GetPostByUrlAsync(System.String)` -- single post lookup
  - `M:Pennington.BlogSite.Services.BlogContentResolver.GetTagsWithCountsAsync` -- tags with their post counts
  - `M:Pennington.BlogSite.Services.BlogContentResolver.GetPostsByTagAsync(System.String)` -- posts filtered by a specific tag
- Sitemaps are powered by Pennington core's `T:Pennington.Feeds.SitemapService` (registered in `AddPennington`). RSS feed generation is not yet complete -- `T:Pennington.Feeds.RssFeedBuilder` is registered but no endpoint serves the feed
- The `P:Pennington.BlogSite.BlogSiteOptions.SocialMediaImageUrlFactory` property can be set to a function that generates per-post social images from a `T:Pennington.BlogSite.BlogPostPage`
- `P:Pennington.BlogSite.BlogSiteOptions.EnableSitemap` (default `true`) controls sitemap generation

## Beat 8: What's next

The reader has a working blog and is pointed to further resources. The goal is to connect this tutorial to customization guides and deployment.

### What to show
- Summary: in ~20 lines of C# and 3 markdown files, the reader has a blog with home page, post pages, tag pages, series linking, RSS, and sitemap
- Link to "Deploying to GitHub Pages" to put the blog live
- Link to DocSite Quick Start for documentation sites
- For color scheme, font, and layout customization, see the Configuring BlogSite how-to.

### Key points
- BlogSite and DocSite are peers -- both build on Pennington core, but for different use cases
- For a site that combines documentation and a blog, the reader could use Pennington core directly with two `AddMarkdownContent` calls (one for docs, one for blog) and build the layout themselves
- The `dotnet run -- build` command works the same way for static site generation

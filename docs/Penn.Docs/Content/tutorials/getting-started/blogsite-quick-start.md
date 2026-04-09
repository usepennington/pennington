---
title: "BlogSite Quick Start"
description: "Launch a blog with posts, tags, series, archives, and RSS using the Penn.BlogSite package"
uid: "penn.tutorials.blogsite-quick-start"
order: 30
---

## Beat 1: Create the project and install Penn.BlogSite

The reader scaffolds an empty ASP.NET project and adds the BlogSite package. The goal is to show that BlogSite is a single-package solution for blogs, parallel to DocSite for documentation.

### What to show
- Terminal commands: `dotnet new web -n AlexBlog`, then `dotnet add package Penn.BlogSite`
- Brief explanation: `Penn.BlogSite` bundles Penn core, MonorailCSS, and a blog-specific layout with home page, post pages, tag pages, and archive
- Show the `.csproj` with the single PackageReference

### Key points
- Like DocSite, BlogSite is a convenience layer -- it internally registers Penn, MonorailCSS, and its own Razor components
- BlogSite provides: home page with hero and project showcase, blog post listing, individual post pages, tag pages, date-based ordering, RSS feed, and sitemap
- No layout component needs to be created by the reader

## Beat 2: Write Program.cs with BlogSiteOptions

The reader writes ~10 lines of Program.cs configuring the blog's identity and author info. The goal is to get a running blog with the minimum viable options.

### What to show
- Complete Program.cs (~10 lines) using:
  - `M:Penn.BlogSite.BlogSiteServiceExtensions.AddBlogSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Penn.BlogSite.BlogSiteOptions})` -- takes a factory function returning `BlogSiteOptions`
  - `M:Penn.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)` -- configures middleware
  - `M:Penn.BlogSite.BlogSiteServiceExtensions.RunBlogSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` -- delegates to `RunOrBuildAsync`
- The `BlogSiteOptions` record configured with:
  - `P:Penn.BlogSite.BlogSiteOptions.SiteTitle` -- `"Alex's Dev Blog"` (required)
  - `P:Penn.BlogSite.BlogSiteOptions.Description` -- `"Notes on .NET, tools, and developer life"` (required)
  - `P:Penn.BlogSite.BlogSiteOptions.AuthorName` -- `"Alex Chen"`
  - `P:Penn.BlogSite.BlogSiteOptions.AuthorBio` -- `"Software engineer who writes about .NET, developer tools, and the occasional hot take."`
- Code reference: `T:Penn.BlogSite.BlogSiteOptions`

### Key points
- `AddBlogSite` is a single package that bundles Penn, MonorailCSS, and blog components
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
- Code reference for the front matter type: `T:Penn.BlogSite.BlogSiteFrontMatter`

### Key points
- `BlogSiteFrontMatter` implements: `T:Penn.FrontMatter.IFrontMatter`, `T:Penn.FrontMatter.IDraftable`, `T:Penn.FrontMatter.ITaggable`, `T:Penn.FrontMatter.IDescribable`, `T:Penn.FrontMatter.IDateable`, `T:Penn.FrontMatter.ICrossReferenceable`, `T:Penn.FrontMatter.ISectionable`, `T:Penn.FrontMatter.IRedirectable`
- Key fields unique to blog posts:
  - `P:Penn.BlogSite.BlogSiteFrontMatter.Date` -- a `DateTime?` used for ordering (newest first) and display
  - `P:Penn.BlogSite.BlogSiteFrontMatter.Author` -- displayed on the post page
  - `P:Penn.BlogSite.BlogSiteFrontMatter.Series` -- a string that groups posts into a series (more on this in Beat 4)
  - `P:Penn.BlogSite.BlogSiteFrontMatter.Repository` -- optional link to a code repository for the post
  - `P:Penn.BlogSite.BlogSiteFrontMatter.Tags` -- string array for categorization; each tag generates a tag page at `{TagsPageUrl}/{encoded-tag-name}`
  - `P:Penn.BlogSite.BlogSiteFrontMatter.IsDraft` -- set to `true` to exclude a post from the published site
- Contrast with the core `T:Penn.FrontMatter.BlogFrontMatter` which is the base version without `RedirectUrl`, `Section`, or `Repository` -- `BlogSiteFrontMatter` is the full-featured variant used by the BlogSite package
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
- The series mechanism: `T:Penn.BlogSite.Services.BlogContentResolver` queries all posts via `M:Penn.BlogSite.Services.BlogContentResolver.GetAllPostsAsync` and groups by the `Series` property on `BlogSiteFrontMatter`

### Key points
- Series linking is automatic -- any two or more posts with the same `series` string value are grouped and linked chronologically by date
- `T:Penn.BlogSite.Services.BlogContentResolver` is the service responsible for resolving blog posts, computing tags, and grouping series
- `BlogContentResolver` is registered as a file-watched service (`AddFileWatched<BlogContentResolver>`) so it re-scans content when files change
- `T:Penn.BlogSite.BlogPostPage` is the resolved post model: it has `FrontMatter` (the `BlogSiteFrontMatter`), `Url`, and `Tags` (array of `T:Penn.BlogSite.BlogTag`)
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
- The `[!TIP]` alert demonstrates that Penn's markdown alert extension works in blog posts just as in documentation pages
- Tags are URL-encoded for the tag page URL: `BlogTag` (`T:Penn.BlogSite.BlogTag`) has `Name` (display) and `Url` (e.g., `/tags/dotnet`)

## Beat 6: Personalize the home page

The reader adds the hero section, social links, and project showcase to BlogSiteOptions. The goal is to show how the home page comes alive with these additions.

### What to show
- Return to `Program.cs` and add to `BlogSiteOptions`:
  - `P:Penn.BlogSite.BlogSiteOptions.HeroContent` -- `new HeroContent("Hi, I'm Alex", "I write about .NET, developer tools, and making software that doesn't make you cry.")`
  - `P:Penn.BlogSite.BlogSiteOptions.Socials` -- array of `T:Penn.BlogSite.SocialLink` with GitHub and Mastodon entries (each takes an SVG `RenderFragment` and a URL)
  - `P:Penn.BlogSite.BlogSiteOptions.MyWork` -- array of `T:Penn.BlogSite.Project`: `new Project("Tempo", "Task scheduling for .NET", "https://github.com/...")`
  - `P:Penn.BlogSite.BlogSiteOptions.CanonicalBaseUrl` -- `"https://alexchen.dev"` (needed for RSS and sitemap absolute URLs)
- Code reference: `T:Penn.BlogSite.HeroContent`, `T:Penn.BlogSite.SocialLink`, `T:Penn.BlogSite.Project`
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
  3. **Tag pages** -- click a tag (e.g., "dotnet") to see the tag page at `/tags/dotnet` listing all posts with that tag. Navigate to `/tags/` to see all tags with post counts (via `M:Penn.BlogSite.Services.BlogContentResolver.GetTagsWithCountsAsync`)
  4. **Date ordering** -- posts appear newest first (the Linux post from April 1, then CLI Part 2 from March 22, then Part 1 from March 15)
  5. **RSS feed** -- note that `P:Penn.BlogSite.BlogSiteOptions.EnableRss` (default `true`) adds a `<link type="application/rss+xml">` tag pointing to `/rss.xml` in the HTML head. RSS feed serving is not yet implemented in Penn core -- the `T:Penn.Feeds.RssFeedBuilder` is registered but no endpoint serves the XML. This is tracked as a known gap
  6. **Sitemap** -- visit `/sitemap.xml` to see the auto-generated sitemap
  7. **Styles** -- note the MonorailCSS styling, dark mode toggle, syntax highlighting in code blocks

### Key points
- The blog home page layout, post layout, tag pages, and archive pages are all provided by BlogSite's Razor components in `src/Penn.BlogSite/Components/`
- `BlogContentResolver` powers most of the data:
  - `M:Penn.BlogSite.Services.BlogContentResolver.GetAllPostsAsync` -- all posts ordered by date descending
  - `M:Penn.BlogSite.Services.BlogContentResolver.GetPostByUrlAsync(System.String)` -- single post lookup
  - `M:Penn.BlogSite.Services.BlogContentResolver.GetTagsWithCountsAsync` -- tags with their post counts
  - `M:Penn.BlogSite.Services.BlogContentResolver.GetPostsByTagAsync(System.String)` -- posts filtered by a specific tag
- Sitemaps are powered by Penn core's `T:Penn.Feeds.SitemapService` (registered in `AddPenn`). RSS feed generation is not yet complete -- `T:Penn.Feeds.RssFeedBuilder` is registered but no endpoint serves the feed
- The `P:Penn.BlogSite.BlogSiteOptions.SocialMediaImageUrlFactory` property can be set to a function that generates per-post social images from a `T:Penn.BlogSite.BlogPostPage`
- `P:Penn.BlogSite.BlogSiteOptions.EnableSitemap` (default `true`) controls sitemap generation

## Beat 8: What's next

The reader has a working blog and is pointed to further resources. The goal is to connect this tutorial to customization guides and deployment.

### What to show
- Summary: in ~20 lines of C# and 3 markdown files, the reader has a blog with home page, post pages, tag pages, series linking, RSS, and sitemap
- Link to "Deploying to GitHub Pages" to put the blog live
- Link to DocSite Quick Start for documentation sites
- For color scheme, font, and layout customization, see the Configuring BlogSite how-to.

### Key points
- BlogSite and DocSite are peers -- both build on Penn core, but for different use cases
- For a site that combines documentation and a blog, the reader could use Penn core directly with two `AddMarkdownContent` calls (one for docs, one for blog) and build the layout themselves
- The `dotnet run -- build` command works the same way for static site generation

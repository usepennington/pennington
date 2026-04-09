---
title: "Configuring BlogSite"
description: "Configure BlogSiteOptions: blog content paths, author info, hero section, social links, project showcase, RSS and sitemap toggles, series support, tag pages, and social media image generation"
uid: "penn.how-to.configuring-blogsite"
order: 20
---

## Beat 1: Start with a minimal BlogSite

The reader creates a bare `Program.cs` with only the required properties plus author info. Running the site shows the blog shell: a home page with no hero, no social links, and no projects -- just the site title and an empty post list.

### What to show
- The `AddBlogSite` extension method: `M:Penn.BlogSite.BlogSiteServiceExtensions.AddBlogSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Penn.BlogSite.BlogSiteOptions})`
- Set the two `required` properties: `P:Penn.BlogSite.BlogSiteOptions.SiteTitle` and `P:Penn.BlogSite.BlogSiteOptions.Description`
- Set optional author properties: `P:Penn.BlogSite.BlogSiteOptions.AuthorName` to `"Mara Chen"` and `P:Penn.BlogSite.BlogSiteOptions.AuthorBio` to `"Staff engineer focused on .NET performance"`
- The middleware chain: `M:Penn.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)` and `M:Penn.BlogSite.BlogSiteServiceExtensions.RunBlogSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])`
- A minimal `Program.cs`:
  ```csharp
  builder.Services.AddBlogSite(() => new BlogSiteOptions
  {
      SiteTitle = "Mara Writes Code",
      Description = "Performance engineering for .NET",
      AuthorName = "Mara Chen",
  });
  ```

### Key points
- `SiteTitle` and `Description` are the only `required` properties on `T:Penn.BlogSite.BlogSiteOptions`
- `P:Penn.BlogSite.BlogSiteOptions.ContentRootPath` defaults to `"Content"` and `P:Penn.BlogSite.BlogSiteOptions.BlogContentPath` defaults to `"Blog"` -- so posts are expected at `Content/Blog/` by default
- `P:Penn.BlogSite.BlogSiteOptions.BlogBaseUrl` defaults to `"/blog"` and `P:Penn.BlogSite.BlogSiteOptions.TagsPageUrl` defaults to `"/tags"`
- Unlike DocSite, BlogSite does not register SPA navigation or llms.txt -- it uses `T:Penn.BlogSite.Services.BlogContentResolver` as its main content query service

---

## Beat 2: Configure content paths and add posts

The reader changes the default content paths to match Mara's preferred directory structure, then adds two blog posts with `BlogSiteFrontMatter` YAML. Running the site shows posts at `/blog/` and tags at `/topics/`.

### What to show
- Set `P:Penn.BlogSite.BlogSiteOptions.BlogContentPath` to `"Posts"`
- Set `P:Penn.BlogSite.BlogSiteOptions.BlogBaseUrl` to `"/blog"`
- Set `P:Penn.BlogSite.BlogSiteOptions.TagsPageUrl` to `"/topics"`
- Show how `AddBlogSite` internally constructs the content path: `Path.Combine(options.ContentRootPath, options.BlogContentPath)` becomes `"Content/Posts"`, and registers it via `M:Penn.Infrastructure.PennOptions.AddMarkdownContent``1(System.Action{Penn.Infrastructure.MarkdownContentOptions})` with `T:Penn.BlogSite.BlogSiteFrontMatter`
- Show the full `T:Penn.BlogSite.BlogSiteFrontMatter` record with all its properties and implemented interfaces: `T:Penn.FrontMatter.IFrontMatter`, `T:Penn.FrontMatter.IDraftable`, `T:Penn.FrontMatter.ITaggable`, `T:Penn.FrontMatter.IDescribable`, `T:Penn.FrontMatter.IDateable`, `T:Penn.FrontMatter.ICrossReferenceable`, `T:Penn.FrontMatter.ISectionable`, `T:Penn.FrontMatter.IRedirectable`
- Create two markdown files under `Content/Posts/` with YAML front matter using `P:Penn.BlogSite.BlogSiteFrontMatter.Title`, `P:Penn.BlogSite.BlogSiteFrontMatter.Date`, `P:Penn.BlogSite.BlogSiteFrontMatter.Tags`, `P:Penn.BlogSite.BlogSiteFrontMatter.Description`, and `P:Penn.BlogSite.BlogSiteFrontMatter.Series`

### Key points
- `BlogSiteFrontMatter` has properties that DocSiteFrontMatter lacks: `P:Penn.BlogSite.BlogSiteFrontMatter.Author`, `P:Penn.BlogSite.BlogSiteFrontMatter.Date` (via `T:Penn.FrontMatter.IDateable`), `P:Penn.BlogSite.BlogSiteFrontMatter.Repository`, and `P:Penn.BlogSite.BlogSiteFrontMatter.Series`
- `BlogBaseUrl` determines the URL prefix for all posts -- a post at `Content/Posts/my-post.md` maps to `/blog/my-post/`
- `TagsPageUrl` determines where tag listing pages render -- each tag gets a page at `{TagsPageUrl}/{encoded-tag-name}`
- The `T:Penn.BlogSite.Services.BlogContentResolver` uses `M:Penn.BlogSite.Services.BlogContentResolver.GetAllPostsAsync` to return posts ordered by date descending

---

## Beat 3: Build the home page sections

The reader adds the four home-page components: a hero section with title and description, a project showcase, social links with SVG icons, and header navigation links. After running, the home page fills in with all sections.

### What to show
- Set `P:Penn.BlogSite.BlogSiteOptions.HeroContent` with a `T:Penn.BlogSite.HeroContent` record:
  ```csharp
  HeroContent = new HeroContent("Hi, I'm Mara", "I write about making .NET applications <em>fast</em>."),
  ```
- Show the `T:Penn.BlogSite.HeroContent` record: `HeroContent(string Title, string Description)`
- Set `P:Penn.BlogSite.BlogSiteOptions.MyWork` with `T:Penn.BlogSite.Project` entries:
  ```csharp
  MyWork = [
      new Project("HotPath", "Allocation-free HTTP pipeline toolkit", "https://github.com/mara/hotpath"),
      new Project("BenchTool", "Micro-benchmark runner for .NET", "https://github.com/mara/benchtool"),
  ],
  ```
- Show the `T:Penn.BlogSite.Project` record: `Project(string Title, string Description, string Url)`
- Set `P:Penn.BlogSite.BlogSiteOptions.Socials` with `T:Penn.BlogSite.SocialLink` entries (note: `Icon` is a `RenderFragment`):
  ```csharp
  Socials = [
      new SocialLink(@<svg>...</svg>, "https://github.com/mara"),
      new SocialLink(@<svg>...</svg>, "https://mastodon.social/@mara"),
  ],
  ```
- Show the `T:Penn.BlogSite.SocialLink` record: `SocialLink(RenderFragment Icon, string Url)`
- Set `P:Penn.BlogSite.BlogSiteOptions.MainSiteLinks` with `T:Penn.BlogSite.HeaderLink` entries:
  ```csharp
  MainSiteLinks = [new HeaderLink("Talks", "https://mara.dev/talks")],
  ```
- Show the `T:Penn.BlogSite.HeaderLink` record: `HeaderLink(string Title, string Url)`

### Key points
- `HeroContent.Description` accepts raw HTML -- the `<em>` tag in the example is rendered as markup via `@((MarkupString)BlogOptions.HeroContent.Description)` in the Home.razor component
- `SocialLink.Icon` is a `RenderFragment` (not a string) -- in Razor syntax, use `@<svg>...</svg>` to inline SVG icons
- `MyWork` entries render as clickable project cards on the home page
- `MainSiteLinks` render as navigation links in the header -- useful for linking to external pages (talks, portfolio, etc.)
- All four properties default to empty arrays/null, so the home page sections only appear when configured

---

## Beat 4: Set up branding and typography

The reader applies Mara's personal brand: a warm hue via `AlgorithmicColorScheme`, custom display and body fonts, font preloads, and extra CSS. After running, the entire site reflects the new visual identity.

### What to show
- Set `P:Penn.BlogSite.BlogSiteOptions.ColorScheme` to a `T:Penn.MonorailCss.AlgorithmicColorScheme`:
  ```csharp
  ColorScheme = new AlgorithmicColorScheme { PrimaryHue = 25 },
  ```
- Set `P:Penn.BlogSite.BlogSiteOptions.DisplayFontFamily` to `"'Playfair Display', serif"`
- Set `P:Penn.BlogSite.BlogSiteOptions.BodyFontFamily` to `"'Inter', sans-serif"`
- Set `P:Penn.BlogSite.BlogSiteOptions.FontPreloads` with `T:Penn.Infrastructure.FontPreload` entries
- Set `P:Penn.BlogSite.BlogSiteOptions.ExtraStyles` with `@font-face` declarations
- Set `P:Penn.BlogSite.BlogSiteOptions.AdditionalHtmlHeadContent` for any additional meta tags

### Key points
- BlogSite passes `ColorScheme` through to `T:Penn.MonorailCss.MonorailCssOptions` in its `AddBlogSite` implementation -- if null, it falls back to the default `T:Penn.MonorailCss.NamedColorScheme` (Blue/Purple/Cyan/Pink/Slate)
- The branding properties (`DisplayFontFamily`, `BodyFontFamily`, `FontPreloads`, `ExtraStyles`, `AdditionalHtmlHeadContent`) work identically to their DocSite counterparts
- A warm hue like 25 (orange-amber) creates an inviting personal brand; the `AlgorithmicColorScheme` auto-generates complementary accent and tertiary colors

---

## Beat 5: Enable feeds and SEO

The reader confirms that RSS and sitemap are enabled by default, then sets `CanonicalBaseUrl` so generated URLs are absolute. Visiting `/feed.xml` and `/sitemap.xml` confirms the feeds render correctly.

### What to show
- Show that `P:Penn.BlogSite.BlogSiteOptions.EnableRss` defaults to `true`
- Show that `P:Penn.BlogSite.BlogSiteOptions.EnableSitemap` defaults to `true`
- Set `P:Penn.BlogSite.BlogSiteOptions.CanonicalBaseUrl` to `"https://mara.dev/"`
- Explain that `CanonicalBaseUrl` flows through to `T:Penn.Infrastructure.PennOptions` where it is used by the `T:Penn.Feeds.SitemapBuilder` and `T:Penn.Feeds.RssFeedBuilder` to produce absolute URLs
- Verify `/sitemap.xml` and the search index at `/search-index.json` are generated

### Key points
- Both `EnableRss` and `EnableSitemap` default to `true` -- set them to `false` only if you explicitly want to disable feed generation
- Without `CanonicalBaseUrl`, feed URLs are relative paths, which breaks most RSS readers and search engine crawlers
- The sitemap and RSS endpoints are registered by `M:Penn.Infrastructure.PennExtensions.UsePenn(Microsoft.AspNetCore.Builder.WebApplication)` -- BlogSite calls this internally via `UseBlogSite`

---

## Beat 6: Add social media image generation

The reader wires up the `SocialMediaImageUrlFactory` callback to generate Open Graph image URLs for each post. The callback receives a `BlogPostPage` and returns a URL string.

### What to show
- Set `P:Penn.BlogSite.BlogSiteOptions.SocialMediaImageUrlFactory`:
  ```csharp
  SocialMediaImageUrlFactory = post =>
      $"https://og.mara.dev/api/image?title={Uri.EscapeDataString(post.FrontMatter.Title)}",
  ```
- Show the `T:Penn.BlogSite.BlogPostPage` record: `BlogPostPage(BlogSiteFrontMatter FrontMatter, string Url, BlogTag[] Tags)`
- Show the `T:Penn.BlogSite.BlogTag` record: `BlogTag(string Name, string Url)`
- Show how the Blog.razor component uses the factory: `:path src/Penn.BlogSite/Components/Pages/Blog.razor` -- the `og:image` meta tag is conditionally rendered when `SocialMediaImageUrlFactory` is not null

### Key points
- `SocialMediaImageUrlFactory` is `Func<BlogPostPage, string>?` -- it receives the full `BlogPostPage` with access to `P:Penn.BlogSite.BlogPostPage.FrontMatter` (all front matter fields), `P:Penn.BlogSite.BlogPostPage.Url` (canonical URL), and `P:Penn.BlogSite.BlogPostPage.Tags` (resolved tag array)
- The factory is called per-post in the Blog.razor page component to set `og:image`, `twitter:image`, and related meta tags
- A common pattern is to point at an external OG image generation service (like Vercel OG or a custom endpoint) that renders the title as an image
- When `SocialMediaImageUrlFactory` is null, no `og:image` meta tag is emitted

---

## Beat 7: Observe automatic features -- series, tags, and archive

The reader adds a third blog post that shares a `Series` value with one of the existing posts, then browses to see automatic series navigation, tag listing pages, and the archive view.

### What to show
- Add a third markdown file with `P:Penn.BlogSite.BlogSiteFrontMatter.Series` set to `"Zero-Alloc .NET"` (matching an earlier post)
- Show `T:Penn.BlogSite.BlogSiteFrontMatter` with the `P:Penn.BlogSite.BlogSiteFrontMatter.Series` property highlighted
- Show how `T:Penn.BlogSite.Services.BlogContentResolver` provides tag queries: `M:Penn.BlogSite.Services.BlogContentResolver.GetTagsWithCountsAsync` and `M:Penn.BlogSite.Services.BlogContentResolver.GetPostsByTagAsync(System.String)`
- Show the `T:Penn.BlogSite.RenderedBlogPost` record: `RenderedBlogPost(BlogPostPage Page, string Html)` -- used by `M:Penn.BlogSite.Services.BlogContentResolver.GetPostByUrlAsync(System.String)`
- Verify tag pages at `/topics/performance` list posts with that tag
- Verify the home page shows recent posts via the `Home.razor` component: `:path src/Penn.BlogSite/Components/Pages/Home.razor`

### Key points
- Posts with the same `Series` value are automatically linked with series navigation (previous/next in series)
- `BlogContentResolver.GetAllPostsAsync` returns all non-draft posts ordered by `Date` descending -- the home page shows the 5 most recent
- `BlogContentResolver.GetTagsWithCountsAsync` returns all unique tags with post counts, ordered by count descending
- `BlogContentResolver.GetPostsByTagAsync` takes a URL-encoded tag name and returns matching posts
- `BlogSiteFrontMatter.IsDraft` (from `T:Penn.FrontMatter.IDraftable`) hides posts from all listings and feeds when `true`
- The `P:Penn.BlogSite.BlogSiteOptions.AdditionalRoutingAssemblies` property works identically to DocSite's -- it lets Razor pages from companion projects participate in routing

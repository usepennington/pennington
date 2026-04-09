---
title: "Generating RSS Feeds and Sitemaps"
description: "Configure CanonicalBaseUrl, generate RSS feeds with RssFeedBuilder, and XML sitemaps with SitemapBuilder including multi-locale hreflang annotations"
uid: "penn.how-to.generating-rss-feeds-and-sitemaps"
order: 10
---

You want search engines and feed readers to discover your content automatically. Penn generates RSS feeds and XML sitemaps from your content metadata.

## Beat 1: Prerequisites -- CanonicalBaseUrl and IDateable front matter

Before RSS or sitemap generation works, the site needs a canonical base URL (for absolute URLs in feeds and sitemaps) and content with publication dates (for RSS ordering). This beat establishes both.

### What to show
- Show setting `P:Penn.Infrastructure.PennOptions.CanonicalBaseUrl` in `Program.cs`. For BlogSite, this is `P:Penn.BlogSite.BlogSiteOptions.CanonicalBaseUrl`. Explain that this value is used by `T:Penn.Feeds.RssFeedBuilder` and `T:Penn.Feeds.SitemapBuilder` to construct absolute URLs.
- Show how `CanonicalBaseUrl` flows into the feed builders: in `M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})`, the canonical base is wrapped as `T:Penn.Routing.UrlPath` and injected into both `new SitemapBuilder(canonicalBase)` and `new RssFeedBuilder(canonicalBase)`.
- Explain the `T:Penn.FrontMatter.IDateable` interface: a single property `P:Penn.FrontMatter.IDateable.Date` (`DateTime?`). RSS requires this to sort and filter items. Show that `T:Penn.BlogSite.BlogSiteFrontMatter` implements `IDateable` along with `T:Penn.FrontMatter.IDraftable`, `T:Penn.FrontMatter.IDescribable`, `T:Penn.FrontMatter.ITaggable`, `T:Penn.FrontMatter.ICrossReferenceable`, `T:Penn.FrontMatter.ISectionable`, and `T:Penn.FrontMatter.IRedirectable`.
- Show a sample blog post front matter with `date: 2026-03-15` to confirm it satisfies `IDateable`.

### Key points
- Without `CanonicalBaseUrl`, feed URLs will be relative (defaulting to `/`) which produces invalid RSS and sitemap entries
- `IDateable` is a capability interface -- any front matter type can implement it, not just `BlogSiteFrontMatter`
- BlogSite sets `CanonicalBaseUrl` automatically when the option is provided: `P:Penn.BlogSite.BlogSiteOptions.CanonicalBaseUrl` flows through to `P:Penn.Infrastructure.PennOptions.CanonicalBaseUrl`

## Beat 2: RSS feed generation (planned)

**Note:** RSS feed serving is not yet complete in Penn. `T:Penn.Feeds.RssFeedBuilder` is registered by `AddPenn` and its `Build` method produces `ImmutableList<RssFeedItem>`, but no endpoint serves the RSS XML. BlogSite's `App.razor` includes a `<link>` tag pointing to `/rss.xml`, but this URL returns 404. This section documents the intended design for when RSS serving is implemented.

### What to show
- Show `T:Penn.Feeds.RssFeedBuilder` and its constructor taking `T:Penn.Routing.UrlPath` for the canonical base. Walk through `M:Penn.Feeds.RssFeedBuilder.Build(System.Collections.Generic.IReadOnlyList{Penn.Pipeline.RenderedItem})`: it iterates rendered items, skips drafts (via `T:Penn.FrontMatter.IDraftable` check for `IsDraft: true`), collects items that implement `T:Penn.FrontMatter.IDateable` with a non-null `Date`, sorts by date descending, and builds `T:Penn.Feeds.RssFeedItem` records.
- Show the `T:Penn.Feeds.RssFeedItem` record: `Title` (from `P:Penn.FrontMatter.IFrontMatter.Title`), `Description` (from `T:Penn.FrontMatter.IDescribable` if implemented), `Url` (absolute via `ContentRoute.AbsoluteUrl`), `PublishDate`, and `Author` (currently `null` -- reserved for future use).
- Note that `P:Penn.BlogSite.BlogSiteOptions.EnableRss` defaults to `true` and `App.razor` adds a `<link>` tag for `/rss.xml`, but the endpoint itself needs to be implemented.

### Key points
- `RssFeedBuilder.Build` produces `ImmutableList<RssFeedItem>` but no code currently serializes these to RSS XML or serves them via an endpoint
- Only content implementing both `IDateable` (with a non-null date) and not marked as draft is included
- The `IDescribable.Description` property populates the RSS item's `<description>` element

## Beat 3: XML sitemap generation with SitemapBuilder and SitemapService

How Penn generates a standards-compliant sitemap XML file, including multi-locale support with hreflang annotations.

### What to show
- Show `T:Penn.Feeds.SitemapService`: it queries all `T:Penn.Content.IContentService` instances for TOC entries, constructs absolute URLs using the `P:Penn.Feeds.SitemapBuilder.CanonicalBase` property. Note that `T:Penn.Feeds.SitemapBuilder` also has a `Build` method that operates on `RenderedItem` objects, but the actual sitemap generation uses `SitemapService` which works from `ContentTocItem` records directly. Drafts are excluded upstream by `MarkdownContentService.GetContentTocEntriesAsync`.
- Show the `T:Penn.Feeds.SitemapEntry` record: `Url` (`UrlPath`), `LastModified` (`DateTime?`), `ChangeFrequency` (`string?`), and `Priority` (`double?`).
- Show `T:Penn.Feeds.SitemapService`: it uses `AsyncLazy` for lazy computation, queries all `T:Penn.Content.IContentService` instances for TOC entries, and serializes to XML using `System.Xml.Linq`. When `T:Penn.Infrastructure.LocalizationOptions.IsMultiLocale` is true, it generates `<xhtml:link rel="alternate" hreflang="..." href="..."/>` elements for each locale.
- Show the endpoint mapping in `UsePenn`: `app.MapGet("/sitemap.xml", async (Feeds.SitemapService service) => Results.Content(await service.GetSitemapXmlAsync(), "application/xml"))`.
- Note that `SitemapService` is registered via `services.AddFileWatched<Feeds.SitemapService>()` so it automatically rebuilds when content files change.

### Key points
- The sitemap is generated lazily on first request and cached until content files change (managed by `T:Penn.Infrastructure.FileWatchDependencyFactory{T}`)
- Drafts are excluded from the sitemap
- Multi-locale sites get hreflang annotations automatically -- the `SitemapService` builds a locale-to-URL map and adds `<xhtml:link>` elements for pages available in multiple languages
- The sitemap follows the `http://www.sitemaps.org/schemas/sitemap/0.9` schema

## Beat 4: Verify feeds in dev and build

How to confirm that RSS feeds and sitemaps are working correctly during development and in the static build output.

### What to show
- During development with `dotnet run`: navigate to `/sitemap.xml` and inspect the XML for correct content. Note that `/rss.xml` is not yet served (see Beat 2).
- Run `dotnet run -- build` to generate the static site. Confirm that the output directory contains `sitemap.xml`.
- Show how the static site builder discovers endpoints: `T:Penn.Generation.OutputGenerationService` scans `EndpointDataSource` for MapGet routes (like `/sitemap.xml`, `/styles.css`, `/search-index.json`) and fetches them during the build.

### Key points
- The RSS feed requires both `CanonicalBaseUrl` and at least one content item implementing `IDateable` with a non-null date to produce any items
- The sitemap includes all non-draft content pages regardless of whether they implement `IDateable`

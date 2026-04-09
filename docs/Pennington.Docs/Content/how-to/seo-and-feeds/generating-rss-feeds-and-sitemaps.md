---
title: "Generating RSS Feeds and Sitemaps"
description: "Configure CanonicalBaseUrl, generate RSS feeds with RssFeedBuilder, and XML sitemaps with SitemapBuilder including multi-locale hreflang annotations"
uid: "penn.how-to.generating-rss-feeds-and-sitemaps"
order: 10
---

You want search engines and feed readers to discover your content automatically. Pennington generates RSS feeds and XML sitemaps from your content metadata.

## Beat 1: Prerequisites -- CanonicalBaseUrl and IDateable front matter

Before RSS or sitemap generation works, the site needs a canonical base URL (for absolute URLs in feeds and sitemaps) and content with publication dates (for RSS ordering). This beat establishes both.

### What to show
- Show setting `P:Pennington.Infrastructure.PenningtonOptions.CanonicalBaseUrl` in `Program.cs`. For BlogSite, this is `P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl`. Explain that this value is used by `T:Pennington.Feeds.RssFeedBuilder` and `T:Pennington.Feeds.SitemapBuilder` to construct absolute URLs.
- Show how `CanonicalBaseUrl` flows into the feed builders: in `M:Pennington.Infrastructure.PenningtonExtensions.AddPennington(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Infrastructure.PenningtonOptions})`, the canonical base is wrapped as `T:Pennington.Routing.UrlPath` and injected into both `new SitemapBuilder(canonicalBase)` and `new RssFeedBuilder(canonicalBase)`.
- Explain the `T:Pennington.FrontMatter.IDateable` interface: a single property `P:Pennington.FrontMatter.IDateable.Date` (`DateTime?`). RSS requires this to sort and filter items. Show that `T:Pennington.BlogSite.BlogSiteFrontMatter` implements `IDateable` along with `T:Pennington.FrontMatter.IDraftable`, `T:Pennington.FrontMatter.IDescribable`, `T:Pennington.FrontMatter.ITaggable`, `T:Pennington.FrontMatter.ICrossReferenceable`, `T:Pennington.FrontMatter.ISectionable`, and `T:Pennington.FrontMatter.IRedirectable`.
- Show a sample blog post front matter with `date: 2026-03-15` to confirm it satisfies `IDateable`.

### Key points
- Without `CanonicalBaseUrl`, feed URLs will be relative (defaulting to `/`) which produces invalid RSS and sitemap entries
- `IDateable` is a capability interface -- any front matter type can implement it, not just `BlogSiteFrontMatter`
- BlogSite sets `CanonicalBaseUrl` automatically when the option is provided: `P:Pennington.BlogSite.BlogSiteOptions.CanonicalBaseUrl` flows through to `P:Pennington.Infrastructure.PenningtonOptions.CanonicalBaseUrl`

## Beat 2: RSS feed generation (planned)

**Note:** RSS feed serving is not yet complete in Pennington. `T:Pennington.Feeds.RssFeedBuilder` is registered by `AddPennington` and its `Build` method produces `ImmutableList<RssFeedItem>`, but no endpoint serves the RSS XML. BlogSite's `App.razor` includes a `<link>` tag pointing to `/rss.xml`, but this URL returns 404. This section documents the intended design for when RSS serving is implemented.

### What to show
- Show `T:Pennington.Feeds.RssFeedBuilder` and its constructor taking `T:Pennington.Routing.UrlPath` for the canonical base. Walk through `M:Pennington.Feeds.RssFeedBuilder.Build(System.Collections.Generic.IReadOnlyList{Pennington.Pipeline.RenderedItem})`: it iterates rendered items, skips drafts (via `T:Pennington.FrontMatter.IDraftable` check for `IsDraft: true`), collects items that implement `T:Pennington.FrontMatter.IDateable` with a non-null `Date`, sorts by date descending, and builds `T:Pennington.Feeds.RssFeedItem` records.
- Show the `T:Pennington.Feeds.RssFeedItem` record: `Title` (from `P:Pennington.FrontMatter.IFrontMatter.Title`), `Description` (from `T:Pennington.FrontMatter.IDescribable` if implemented), `Url` (absolute via `ContentRoute.AbsoluteUrl`), `PublishDate`, and `Author` (currently `null` -- reserved for future use).
- Note that `P:Pennington.BlogSite.BlogSiteOptions.EnableRss` defaults to `true` and `App.razor` adds a `<link>` tag for `/rss.xml`, but the endpoint itself needs to be implemented.

### Key points
- `RssFeedBuilder.Build` produces `ImmutableList<RssFeedItem>` but no code currently serializes these to RSS XML or serves them via an endpoint
- Only content implementing both `IDateable` (with a non-null date) and not marked as draft is included
- The `IDescribable.Description` property populates the RSS item's `<description>` element

## Beat 3: XML sitemap generation with SitemapBuilder and SitemapService

How Pennington generates a standards-compliant sitemap XML file, including multi-locale support with hreflang annotations.

### What to show
- Show `T:Pennington.Feeds.SitemapService`: it queries all `T:Pennington.Content.IContentService` instances for TOC entries, constructs absolute URLs using the `P:Pennington.Feeds.SitemapBuilder.CanonicalBase` property. Note that `T:Pennington.Feeds.SitemapBuilder` also has a `Build` method that operates on `RenderedItem` objects, but the actual sitemap generation uses `SitemapService` which works from `ContentTocItem` records directly. Drafts are excluded upstream by `MarkdownContentService.GetContentTocEntriesAsync`.
- Show the `T:Pennington.Feeds.SitemapEntry` record: `Url` (`UrlPath`), `LastModified` (`DateTime?`), `ChangeFrequency` (`string?`), and `Priority` (`double?`).
- Show `T:Pennington.Feeds.SitemapService`: it uses `AsyncLazy` for lazy computation, queries all `T:Pennington.Content.IContentService` instances for TOC entries, and serializes to XML using `System.Xml.Linq`. When `T:Pennington.Infrastructure.LocalizationOptions.IsMultiLocale` is true, it generates `<xhtml:link rel="alternate" hreflang="..." href="..."/>` elements for each locale.
- Show the endpoint mapping in `UsePennington`: `app.MapGet("/sitemap.xml", async (Feeds.SitemapService service) => Results.Content(await service.GetSitemapXmlAsync(), "application/xml"))`.
- Note that `SitemapService` is registered via `services.AddFileWatched<Feeds.SitemapService>()` so it automatically rebuilds when content files change.

### Key points
- The sitemap is generated lazily on first request and cached until content files change (managed by `T:Pennington.Infrastructure.FileWatchDependencyFactory{T}`)
- Drafts are excluded from the sitemap
- Multi-locale sites get hreflang annotations automatically -- the `SitemapService` builds a locale-to-URL map and adds `<xhtml:link>` elements for pages available in multiple languages
- The sitemap follows the `http://www.sitemaps.org/schemas/sitemap/0.9` schema

## Beat 4: Verify feeds in dev and build

How to confirm that RSS feeds and sitemaps are working correctly during development and in the static build output.

### What to show
- During development with `dotnet run`: navigate to `/sitemap.xml` and inspect the XML for correct content. Note that `/rss.xml` is not yet served (see Beat 2).
- Run `dotnet run -- build` to generate the static site. Confirm that the output directory contains `sitemap.xml`.
- Show how the static site builder discovers endpoints: `T:Pennington.Generation.OutputGenerationService` scans `EndpointDataSource` for MapGet routes (like `/sitemap.xml`, `/styles.css`, `/search-index.json`) and fetches them during the build.

### Key points
- The RSS feed requires both `CanonicalBaseUrl` and at least one content item implementing `IDateable` with a non-null date to produce any items
- The sitemap includes all non-draft content pages regardless of whether they implement `IDateable`

---
title: "Publish a custom feed from a content service"
description: "Build the same RSS pattern BlogSite uses for /rss.xml — a content service that caches records, an XML builder method, and a MapGet endpoint — for podcast episodes, conference sessions, changelogs, or any non-blog content type."
uid: how-to.feeds.custom-feed
order: 207025
sectionLabel: "Feeds & Indexes"
tags: [feeds, rss, atom, podcast, content-service]
---

When the records driving a feed are not `BlogSiteFrontMatter`, `BlogSiteOptions.EnableRss` does not apply. Use the same pattern `BlogSite` itself uses: a content service caches the records, exposes a `Task<string>` builder that returns the feed XML, and a `MapGet` endpoint serves it. The static-build crawler picks the endpoint up via `DiscoverMapGetRoutes`, so the feed file lands in `output/` next to every other page — no separate `GetContentToCreateAsync` plumbing.

The reference implementation is `BlogSiteContentService.GetRssXmlAsync` plus the `MapGet` in `UseBlogSite`. This guide names the seams in that pair so the same shape can carry a podcast feed (with the iTunes namespace), an events feed (with iCalendar enclosures), or any custom feed format.

## Before you begin

- A bare `AddPennington` host (see <xref:tutorials.getting-started.first-site>) or any host where the records come from a custom `IContentService` — see <xref:how-to.content-services.custom-content-service> for the discovery shape.
- `CanonicalBaseUrl` set on `PenningtonOptions`, `DocSiteOptions`, or `BlogSiteOptions`. Without it, `<link>` and `<guid>` emit relative URLs that aggregators cannot follow.
- For the BlogSite-shipped `/rss.xml`, use <xref:how-to.feeds.rss> instead — this guide is for the other shapes.

## Build the feed XML on the content service

Add a `Task<string>` method to your content service that orders the cached records and emits XML with `System.Xml.Linq`. `BlogSiteContentService.GetRssXmlAsync` is the reference body:

```csharp:symbol
src/Pennington.BlogSite/Services/BlogSiteContentService.cs > BlogSiteContentService.GetRssXmlAsync
```

The pieces to adapt for your records:

- **The cache.** `_posts` is an `AsyncLazy<ImmutableList<BlogPostDescriptor>>` loaded once per file-watch generation. `DiscoverAsync` and the feed builder share it so the source files read once. For a non-file content service, the same `Lazy<T>` cache that already backs `DiscoverAsync` works without changes.
- **The filter.** BlogSite drops posts without a `Date`. Replace this with whatever predicate keeps an entry in the feed (`IsPublished`, `Status == Released`, future-date skip via `TimeProvider`).
- **The ordering.** Newest-first is conventional for RSS; podcast aggregators expect it.
- **Absolute URLs.** Prefix every `<link>` and `<guid>` with `canonicalBase`. Relative paths break in feed readers.
- **The atom self-link.** `<atom:link rel="self" .../>` tells readers where the feed canonically lives. Match the URL you map below.
- **Per-item elements.** Keep `<title>`, `<link>`, `<guid>`. Add what your content type needs: `<category>` per tag, `<enclosure>` for media attachments, namespaced elements for iTunes/Atom/Dublin Core.

## Wire DI so the endpoint and the discovery list share one instance

Register the concrete service, then forward `IContentService` to the same instance with a transient indirection. Two separate registrations would let the container hand the endpoint a fresh copy with a cold cache:

```csharp
// File-watched when the service reads from disk; AddSingleton<T>() when the
// data source is in-process. The transient IContentService wrapper resolves
// against the current factory-managed instance so file-change recreates flow
// through to both the endpoint and the pipeline.
services.AddFileWatched<PodcastContentService>();
services.AddTransient<IContentService>(sp =>
    sp.GetRequiredService<PodcastContentService>());
```

`AddSingleton<IContentService>` here would cache the first file-watched copy and never refresh — the transient wrapper avoids that trap.

## Map the endpoint

Inject the concrete service into a `MapGet` handler that returns the XML with the right MIME type:

```csharp
app.MapGet("/feed.xml", async (PodcastContentService service) =>
    Results.Content(await service.GetRssXmlAsync(), "application/rss+xml"));
```

Two reasons this single line carries both dev and build:

- **Dev mode** serves `/feed.xml` straight from the handler.
- **Static build** enumerates every `MapGet` endpoint via `DiscoverMapGetRoutes`, fetches each over HTTP through the live pipeline, and writes the body to `output/feed.xml`. No `ContentToCreate` registration is needed.

Reach for `IContentService.GetContentToCreateAsync` instead only when there is no dev-time URL — for example, a `robots.txt` that should not respond live. See <xref:how-to.content-services.emit-generated-artifacts> for that shape.

## Adapt for podcast feeds (iTunes namespace)

A podcast RSS feed extends the same XML with the iTunes namespace plus per-item duration, episode number, and enclosure elements. Declare the namespace on the root and add the children inside the per-item loop:

```csharp
XNamespace atom = "http://www.w3.org/2005/Atom";
XNamespace itunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";

var rss = new XElement("rss",
    new XAttribute("version", "2.0"),
    new XAttribute(XNamespace.Xmlns + "atom", atom.NamespaceName),
    new XAttribute(XNamespace.Xmlns + "itunes", itunes.NamespaceName),
    channel);

// Per-item additions inside the foreach in GetRssXmlAsync:
entry.Add(
    new XElement(itunes + "duration", episode.Duration.ToString(@"hh\:mm\:ss")),
    new XElement(itunes + "episode", episode.EpisodeNumber),
    new XElement(itunes + "season", episode.SeasonNumber),
    new XElement("enclosure",
        new XAttribute("url", absoluteAudioUrl),
        new XAttribute("length", episode.AudioBytes),
        new XAttribute("type", "audio/mpeg")));
```

Channel-level iTunes elements (`<itunes:image>`, `<itunes:category>`, `<itunes:owner>`) sit alongside the existing `<title>` / `<link>` / `<description>` block. Apple's [Podcasters Connect](https://podcasters.apple.com/support/823-podcast-requirements) page is the authoritative list.

## Adapt for Atom feeds

Atom 1.0 uses a different root and element vocabulary. The shape is identical — same cache, same builder method, same `MapGet` — only the XML changes:

```csharp
XNamespace atom = "http://www.w3.org/2005/Atom";

var feed = new XElement(atom + "feed",
    new XElement(atom + "title", _options.SiteTitle),
    new XElement(atom + "id", canonicalBase + "/"),
    new XElement(atom + "updated", DateTime.UtcNow.ToString("o")));

foreach (var entry in ordered)
{
    feed.Add(new XElement(atom + "entry",
        new XElement(atom + "title", entry.Title),
        new XElement(atom + "id", absoluteUrl),
        new XElement(atom + "updated", entry.Date.ToString("o")),
        new XElement(atom + "link", new XAttribute("href", absoluteUrl))));
}
```

Serve at a separate path (`/atom.xml`) with `application/atom+xml`. Nothing stops a site from publishing both RSS and Atom — register two endpoints against the same service.

## Verify

- Run `dotnet run` and fetch `/feed.xml`. The response is the right MIME type with one item per record.
- Run `dotnet run -- build output` and confirm `output/feed.xml` exists with the same body. The build crawler reuses the live endpoint.
- Validate the XML externally — `xmllint --noout feed.xml` catches well-formedness errors. For podcasts, run the file through Apple's podcast validator before submitting to directories.
- Edit a source record and refetch `/feed.xml` in dev. The file-watched cache rebuilds and the change appears without a restart.

## Related

- How-to: [Publish an RSS feed (BlogSite)](xref:how-to.feeds.rss)
- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
- How-to: [Emit generated output artifacts](xref:how-to.content-services.emit-generated-artifacts)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)

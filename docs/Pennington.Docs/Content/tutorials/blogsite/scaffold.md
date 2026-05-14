---
title: "Scaffold a blog with BlogSite"
description: "Swap the bare Pennington host for the BlogSite template and configure the core options that drive the home, archive, post, tag, and RSS routes."
sectionLabel: Getting Started with BlogSite
order: 103010
tags: [blogsite, template, scaffold, options]
uid: tutorials.blogsite.scaffold
---

By the end of this tutorial, a running BlogSite host titled "Scaffold Blog" serves a home listing, `/archive`, `/blog/<slug>/`, `/tags/`, `/tags/<name>/` (plus the `/topics` aliases), and `/rss.xml` — all from a single placeholder post under `Content/Blog/`.

Along the way, you'll see how to swap any plain Pennington host for the BlogSite template in three calls and populate the core `BlogSiteOptions` surface, with a clear mental model of how `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, and `TagsPageUrl` work together.

## Prerequisites

No DocSite experience is required — BlogSite is a separate template. Before starting, gather the following:

- .NET 11 SDK installed
- Completed [Create your first Pennington site](xref:tutorials.getting-started.first-site)
- Completed [Using Blazor Pages](xref:tutorials.getting-started.first-page) (so `Content/` already has at least one markdown file)

The finished code for this tutorial lives in [`examples/BlogSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteScaffoldExample).

---

## 1. Start from the bare Pennington host

The host you built in the getting-started tutorials calls `AddPennington`, registers content with `AddMarkdownContent<DocFrontMatter>`, mounts `UsePennington`, and wires a hand-written `MapGet` fallback that walks `IContentService` to serve individual pages.

<Steps>
<Step StepNumber="1">

**Review the pre-BlogSite host shape**

Here is what that host looks like. The three moving parts are the DI registration, the `UsePennington` call, and the hand-rolled `MapGet` fallback. Notice what is absent: the home listing, `/archive`, `/blog/<slug>` pages, `/tags` and `/topics` aliases, the `/rss.xml` feed, and the MonorailCSS chrome. The next unit brings all of that in with a single `AddBlogSite` call.

```csharp:xmldocid,bodyonly,usings
M:BlogSiteScaffoldExample.Stage1.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/blog/hello-world`
- The page shows unstyled HTML for the markdown — no home listing, no archive, no tag pages, no RSS feed

</Checkpoint>

---

## 2. Swap `AddPennington` for `AddBlogSite`

`AddBlogSite` is the BlogSite template's single registration call. It stands in for `AddPennington` and the `AddMarkdownContent<DocFrontMatter>` line from stage 1, wiring Pennington core, MonorailCSS, the Razor chrome, `BlogContentResolver`, and `BlogSiteContentService` in one call.

<Steps>
<Step StepNumber="1">

**Replace the stage-1 registration with the call below**

The `UsePennington` line and the hand-rolled `MapGet` fallback also come out — `await app.RunAsync()` stands in for them until the next section. The green diff markers show what's new; everything outside them is plain ASP.NET scaffolding.

```csharp:xmldocid,bodyonly,usings
M:BlogSiteScaffoldExample.Stage2.Run(System.String[])
```

The options fall into three families:

- **Site identity** — `SiteTitle`, `Description`, `CanonicalBaseUrl` feed page headers, the RSS channel, sitemap entries, and JSON-LD.
- **Content paths** — `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl` set where posts live on disk and what URLs they get. Shown at their defaults — drop them once the shape is familiar.
- **Author defaults** — `AuthorName` and `AuthorBio` are the site-wide fallbacks for posts that don't set their own `author:` front-matter value.

The full surface, including the homepage knobs `HeroContent`, `MyWork`, `Socials`, and `MainSiteLinks`, lives in <xref:reference.api.blog-site-options>. The next tutorial walks through `BlogSiteFrontMatter`, which the template registers internally in place of the stage-1 `DocFrontMatter`.

</Step>
</Steps>

<Checkpoint>

- `dotnet build` succeeds
- `dotnet run` starts the host, but `/` returns a default ASP.NET response — BlogSite services are registered in DI but the middleware and endpoints are not mounted yet

</Checkpoint>

---

## 3. Mount `UseBlogSite` and swap `RunBlogSiteAsync`

`UseBlogSite` is the middleware counterpart to `AddBlogSite` — one call mounts antiforgery, static files, MonorailCSS, core Pennington middleware, and Razor-component routing for `Home`, `Archive`, `Blog`, `Tag`, and `Tags` in the correct order; when `EnableRss` is true (the default) it also maps `/rss.xml`.

<Steps>
<Step StepNumber="1">

**Call `UseBlogSite` after `Build()`**

This is the middleware counterpart to `AddBlogSite`. After it runs, the BlogSite Razor components own `/`, `/archive`, `/blog/{*fileName}`, `/tags`, `/tags/{TagEncodedName}`, and the `/topics` aliases, with `BlogContentResolver` handling per-request rendering.

```csharp
app.UseBlogSite();
```

</Step>
<Step StepNumber="2">

**Swap `RunAsync` for `RunBlogSiteAsync`**

`RunBlogSiteAsync` delegates to `RunOrBuildAsync`, so the same host serves live in development and generates static HTML when invoked as `dotnet run -- build <baseUrl> <outputDir>`. Both positional arguments are optional and default to `/` and `output` respectively. For the full explanation of how unified dev and build paths work, see <xref:explanation.core.dev-vs-build>.

```csharp
await app.RunBlogSiteAsync(args);
```

</Step>
<Step StepNumber="3">

**See the fully-wired host**

Here is the complete `Program.cs` after the swap. Three calls replace the entire setup from the previous step — the diff says the rest.

```csharp:xmldocid,bodyonly,usings
M:BlogSiteScaffoldExample.Stage3.Run(System.String[])
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- The BlogSite home layout appears: site title "Scaffold Blog", a recent-posts list with one entry, header chrome, and MonorailCSS styling

</Checkpoint>

---

## 4. Drop in a placeholder post and verify every built-in route

Posts live under `{ContentRootPath}/{BlogContentPath}` — with the defaults from step 2, that is `Content/Blog/`. A single placeholder post here keeps the home listing, archive, and RSS feed non-empty until the next tutorial introduces the full `BlogSiteFrontMatter` surface.

<Steps>
<Step StepNumber="1">

**Create `Content/Blog/hello-world.md`**

The placeholder post uses four front-matter keys: `title`, `description`, `date`, and `author`. These are the minimum the home listing and RSS feed need to render an entry. The next tutorial expands this to the full `BlogSiteFrontMatter` surface, adding `tags`, `series`, `repository`, `section`, and `redirectUrl`.

```text:path
examples/BlogSiteScaffoldExample/Content/Blog/hello-world.md
```

</Step>
<Step StepNumber="2">

**Walk the built-in routes**

Visit each URL in order and confirm the placeholder post's metadata appears on every page:

- `/` — home listing with `hello-world` as the only recent post
- `/archive` — full archive (one entry)
- `/blog/hello-world` — the post itself, rendered through the BlogSite post template
- `/tags` — empty tag list (placeholder post has no tags; the next tutorial adds them)
- `/rss.xml` — RSS 2.0 feed with one `<item>` carrying the post title, link, description, pub date, and author
- `/topics` and `/topics/<name>` — aliases for `/tags` and `/tags/<name>` (confirm one loads)

</Step>
</Steps>

<Checkpoint>

- Each URL above returns 200 and renders the placeholder post's metadata
- `/rss.xml` returns `application/rss+xml` content with one item whose `<guid>` matches the canonical post URL

</Checkpoint>

---

## Summary

- The bare `AddPennington` host gave way to `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`, and the full BlogSite chrome now renders.
- The core `BlogSiteOptions` surface — `SiteTitle`, `Description`, `CanonicalBaseUrl`, `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`, `AuthorName`, `AuthorBio` — is populated, and each field flows through to the rendered output.
- BlogSite binds posts through `AddMarkdownContent<BlogSiteFrontMatter>` and defaults content paths to `Content/Blog` served at `/blog`, which distinguishes it from the `DocSite` template's area-driven layout.
- Every built-in route the template ships responds: `/`, `/archive`, `/blog/<slug>`, `/tags` (and `/topics` aliases), `/tags/<name>`, and `/rss.xml`.

---
title: "Scaffold a blog with BlogSite"
description: "Swap the bare Pennington host for the BlogSite template and configure the core options that drive the home, archive, post, tag, and RSS routes."
sectionLabel: Getting Started with BlogSite
order: 103010
tags: [blogsite, template, scaffold, options]
uid: tutorials.blogsite.scaffold
---

By the end of this tutorial you'll have a running BlogSite host titled "Scaffold Blog" that serves a home listing, `/archive`, `/blog/<slug>`, `/tags`, `/tags/<name>` (plus the `/topics` aliases), and `/rss.xml` — all from a single placeholder post under `Content/Blog/`.

You'll walk away able to swap any plain Pennington host for the BlogSite template in three calls and populate the core `BlogSiteOptions` surface, with a clear mental model of how `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, and `TagsPageUrl` work together.

## Prerequisites

You'll need the following before starting. No DocSite experience is required — BlogSite is a separate template.

- .NET 11 SDK installed
- Completed [Create your first Pennington site](xref:tutorials.getting-started.first-site)
- Completed [Add your first markdown page](xref:tutorials.getting-started.first-page) (so `Content/` already has at least one markdown file)

The finished code for this tutorial lives in [`examples/BlogSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteScaffoldExample).

---

## 1. Start from the bare Pennington host

The host you built in the getting-started tutorials calls `AddPennington`, registers content with `AddMarkdownContent<DocFrontMatter>`, mounts `UsePennington`, and wires a hand-written `MapGet` fallback that walks `IContentService` to serve individual pages.

### Step 1.1 — Review the pre-BlogSite host shape

Here is what that host looks like. The three moving parts are the DI registration, the `UsePennington` call, and the hand-rolled `MapGet` fallback. Notice what is absent: the home listing, `/archive`, `/blog/<slug>` pages, `/tags` and `/topics` aliases, the `/rss.xml` feed, and the MonorailCSS chrome. All of that arrives in the next unit with a single `AddBlogSite` call.

```csharp:xmldocid,bodyonly
M:BlogSiteScaffoldExample.Stage1.Run(System.String[])
```

### Checkpoint — The bare host runs

- Run `dotnet run` and visit `http://localhost:5000/blog/hello-world`
- You should see unstyled HTML for your markdown — no home listing, no archive, no tag pages, no RSS feed

---

## 2. Swap `AddPennington` for `AddBlogSite`

`AddBlogSite` is a single DI call that registers Pennington core, MonorailCSS, the Razor-component chrome (home, archive, post, and tag pages), the file-watched `BlogContentResolver`, and the `BlogSiteContentService` that yields per-tag routes and the `/rss.xml` feed — all driven from one options record.

### Step 2.1 — Replace the registration call

`AddBlogSite` takes a `Func<BlogSiteOptions>` — you construct and return a fresh options record rather than mutating one through an `Action`. Remove the `AddMarkdownContent<DocFrontMatter>` call you had before; the template registers `AddMarkdownContent<BlogSiteFrontMatter>` internally, and the next tutorial will walk you through that front-matter record. `AddBlogSite` also calls `AddPennington`, `AddMonorailCss`, and `AddRazorComponents` under the hood, so do not register those separately in any BlogSite project.

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.AddBlogSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.BlogSite.BlogSiteOptions})
```

### Step 2.2 — Populate the core `BlogSiteOptions`

The options this tutorial covers fall into three groups.

The identity trio drives metadata shared across every page and feed: `SiteTitle` is the name that appears in the site header, RSS channel title, and JSON-LD; `Description` populates the RSS channel description and the default meta description; `CanonicalBaseUrl` is the absolute origin (for example `https://myblog.example`) used in RSS `<link>` elements, sitemaps, and JSON-LD `@id` values.

The content-path quartet controls where posts live on disk and what URLs they produce: `ContentRootPath` is the folder relative to `wwwroot` that contains all content (default `"Content"`); `BlogContentPath` is the subfolder within that root where post files live (default `"Blog"`, resolved against `ContentRootPath`); `BlogBaseUrl` is the route prefix for individual post pages (default `"/blog"`); `TagsPageUrl` is the base route for the tag listing and per-tag pages (default `"/tags"`).

`AuthorName` and `AuthorBio` provide site-wide author defaults. They populate the RSS channel, JSON-LD article markup, and any post that omits its own `author:` front-matter field.

The full options surface — including the homepage-specific knobs `HeroContent`, `MyWork`, `Socials`, and `MainSiteLinks` — is covered in <xref:reference.options.blogsite-options>. Those knobs are skipped here and introduced in the third tutorial of this section.

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteOptions
```

### Step 2.3 — Contrast with `DocSite` defaults

Two differences distinguish BlogSite from DocSite at the template level. First, `AddBlogSite` binds `AddMarkdownContent<BlogSiteFrontMatter>` — not `DocSiteFrontMatter` and not the lower-level `BlogFrontMatter` from the core library. Second, BlogSite resolves URLs from a single content-path pair (`ContentRootPath` + `BlogContentPath`) served at a fixed `BlogBaseUrl`, while DocSite drives URLs from `ContentArea` slugs under `ContentRootPath`. The rest of what differs between the two templates — the hard-coded chrome, the RSS-first layout, the absence of an area switcher — follows from those two choices.

### Checkpoint — Services registered, middleware not yet mounted

- `dotnet build` succeeds
- `dotnet run` starts the host, but `/` still returns whatever the pre-BlogSite pipeline produced — BlogSite services are in DI but the middleware and endpoints are not mounted yet

---

## 3. Mount `UseBlogSite` and swap `RunBlogSiteAsync`

`UseBlogSite` is the middleware counterpart to `AddBlogSite` — one call mounts antiforgery, static files, MonorailCSS, core Pennington middleware, and Razor-component routing for `Home`, `Archive`, `Blog`, `Tag`, and `Tags` in the correct order; when `EnableRss` is true (the default) it also maps `/rss.xml`.

### Step 3.1 — Call `UseBlogSite` after `Build()`

This single call replaces both the `UsePennington` line and the hand-written `MapGet` fallback from stage 1. After it runs, the BlogSite Razor components own `/`, `/archive`, `/blog/{*fileName}`, `/tags`, `/tags/{TagEncodedName}`, and the `/topics` aliases, with `BlogContentResolver` handling per-request rendering.

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)
```

### Step 3.2 — Swap `RunAsync` for `RunBlogSiteAsync`

`RunBlogSiteAsync` delegates to `RunOrBuildAsync`, so the same host serves live in development and generates static HTML when you invoke it as `dotnet run -- build <baseUrl> <outputDir>`. Both positional arguments are optional and default to `/` and `output` respectively. For the full explanation of how unified dev and build paths work, see <xref:explanation.core.dev-vs-build>.

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.RunBlogSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

### Step 3.3 — See the fully-wired host

Here is the complete `Program.cs` after the swap. Three calls replace the entire stage-1 setup — the diff says the rest.

```csharp:xmldocid,bodyonly
M:BlogSiteScaffoldExample.Stage2.Run(System.String[])
```

### Checkpoint — Full chrome renders

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see the BlogSite home layout: site title "Scaffold Blog", a recent-posts list with one entry, header chrome, and MonorailCSS styling

---

## 4. Drop in a placeholder post and verify every built-in route

Posts live under `{ContentRootPath}/{BlogContentPath}` — with the defaults from step 2, that is `Content/Blog/`. You'll add a single placeholder post here to keep the home listing, archive, and RSS feed non-empty until the next tutorial introduces the full `BlogSiteFrontMatter` surface.

### Step 4.1 — Create `Content/Blog/hello-world.md`

The placeholder post uses four front-matter keys: `title`, `description`, `date`, and `author`. These are the minimum the home listing and RSS feed need to render an entry. The next tutorial expands this to the full `BlogSiteFrontMatter` surface, adding `tags`, `series`, `repository`, `section`, and `redirectUrl`.

```text:path
examples/BlogSiteScaffoldExample/Content/Blog/hello-world.md
```

### Step 4.2 — Walk the built-in routes

Visit each URL in order and confirm you see the placeholder post's metadata on every page:

- `/` — home listing with `hello-world` as the only recent post
- `/archive` — full archive (one entry)
- `/blog/hello-world` — the post itself, rendered through the BlogSite post template
- `/tags` — empty tag list (placeholder post has no tags; the next tutorial adds them)
- `/rss.xml` — RSS 2.0 feed with one `<item>` carrying the post title, link, description, pub date, and author
- `/topics` and `/topics/<name>` — aliases for `/tags` and `/tags/<name>` (confirm one loads)

### Checkpoint — Every built-in route responds

- Each URL above returns 200 and renders the placeholder post's metadata
- `/rss.xml` returns `application/rss+xml` content with one item whose `<guid>` matches the canonical post URL

---

## Summary

- You replaced the bare `AddPennington` host with `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync` and saw the full BlogSite chrome render.
- You populated the core `BlogSiteOptions` surface — `SiteTitle`, `Description`, `CanonicalBaseUrl`, `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`, `AuthorName`, `AuthorBio` — and watched each field appear in the rendered output.
- You learned that BlogSite binds posts through `AddMarkdownContent<BlogSiteFrontMatter>` and defaults content paths to `Content/Blog` served at `/blog`, distinguishing it from the `DocSite` template's area-driven layout.
- You verified every built-in route the template ships: `/`, `/archive`, `/blog/<slug>`, `/tags` (and `/topics` aliases), `/tags/<name>`, and `/rss.xml`.

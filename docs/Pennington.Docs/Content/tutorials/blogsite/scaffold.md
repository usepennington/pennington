---
title: "Scaffold a blog with BlogSite"
description: "Swap the bare Pennington host for the BlogSite template and configure the core options that drive the home, archive, post, tag, and RSS routes."
sectionLabel: Getting Started with BlogSite
order: 10
tags: [blogsite, template, scaffold, options]
uid: tutorials.blogsite.scaffold
---

> **In this page.** Replace `AddPennington` with `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`, configure the core `BlogSiteOptions` (site title, description, canonical base URL, content paths, author), and see how the BlogSite defaults differ from the `DocSite` template.
>
> **Not in this page.** Authoring individual posts — covered in [Author your first post with BlogSiteFrontMatter](xref:tutorials.blogsite.first-post). Customizing the homepage hero, projects, or social links — covered in [Add a hero, projects, and social links](xref:tutorials.blogsite.hero-projects-socials).

## What you'll do

_**Artifact** (one sentence): describe the concrete output — a running BlogSite host titled "Scaffold Blog" that serves a home listing, `/archive`, `/blog/<slug>`, `/tags`, `/tags/<name>` (plus the `/topics` aliases), and `/rss.xml`, all from a single placeholder post under `Content/Blog/`._

_**Skill** (one sentence): describe what the reader walks away able to do — swap a plain Pennington host for the BlogSite template in three calls, populate the core `BlogSiteOptions` surface, and reason about the `ContentRootPath` / `BlogContentPath` / `BlogBaseUrl` / `TagsPageUrl` defaults._

## Prerequisites

_Keep this list to tools and prior tutorials only. The reader arrives with the bare `AddPennington` host and a `Content/Blog/` folder of markdown already in place from §1.1. No DocSite experience required._

- .NET 11 SDK installed
- Completed [Create your first Pennington site](xref:tutorials.getting-started.first-site)
- Completed [Add your first markdown page](xref:tutorials.getting-started.first-page) (so `Content/` already has at least one markdown file)

The finished code for this tutorial lives in [`examples/BlogSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteScaffoldExample).

---

## 1. Start from the bare Pennington host

_One sentence: remind the reader what their host currently looks like — `AddPennington` with an `AddMarkdownContent<DocFrontMatter>` for `Content/Blog`, `UsePennington`, and a hand-written `MapGet` fallback that walks `IContentService` — so the diff in the next unit is visible._

### Step 1.1 — Review the pre-BlogSite host shape

_Show the starting state verbatim. Call out the three moving parts (DI registration, `UsePennington`, hand-rolled fallback endpoint) without justifying them — this is the shape the reader already built in §1.1. Add a one-sentence note that everything the BlogSite template ships — the home listing, `/archive`, `/blog/<slug>` pages, `/tags` + `/topics` aliases, the `/rss.xml` feed, and the MonorailCSS chrome — is missing here and will be replaced by a single DI call in the next unit._

```csharp:xmldocid,bodyonly
M:BlogSiteScaffoldExample.Stage1.Run(System.String[])
```

### Checkpoint — The bare host runs

- Run `dotnet run` and visit `http://localhost:5000/blog/hello-world`
- You should see unstyled HTML for your markdown — no home listing, no archive, no tag pages, no RSS feed

---

## 2. Swap `AddPennington` for `AddBlogSite`

_One sentence: introduce the BlogSite template as a single DI call that registers Pennington core, MonorailCSS, the Razor-component chrome (home / archive / post / tag pages), the file-watched `BlogContentResolver`, and the `BlogSiteContentService` that yields per-tag routes and the `/rss.xml` feed — all driven from one options record._

### Step 2.1 — Replace the registration call

_Point the reader at the signature: `AddBlogSite` takes a `Func<BlogSiteOptions>` (not an `Action`), so they construct and return a fresh options record. The old `AddMarkdownContent<DocFrontMatter>` block is no longer needed — the template registers `AddMarkdownContent<BlogSiteFrontMatter>` internally and the next tutorial will teach that front-matter record. Note that `AddBlogSite` also calls `AddPennington`, `AddMonorailCss`, and `AddRazorComponents` under the hood; later BlogSite apps in this section must not re-register those._

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.AddBlogSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.BlogSite.BlogSiteOptions})
```

### Step 2.2 — Populate the core `BlogSiteOptions`

_Walk through the nine knobs this tutorial exercises, in two groups. First the identity trio: `SiteTitle`, `Description`, `CanonicalBaseUrl` (used by the RSS channel, sitemap, and JSON-LD). Then the content-path quartet: `ContentRootPath` ("Content"), `BlogContentPath` ("Blog" — relative to `ContentRootPath`), `BlogBaseUrl` ("/blog"), `TagsPageUrl` ("/tags"). Finish with `AuthorName` / `AuthorBio`, which feed the RSS channel, JSON-LD article markup, and any post that omits its own `author:` front-matter value. Point forward to [`BlogSiteOptions` reference](xref:reference.options.blogsite-options) for the full surface — the homepage-specific knobs (`HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`) are deliberately skipped here and covered in the third tutorial of this section._

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteOptions
```

### Step 2.3 — Contrast with `DocSite` defaults

_Two-to-four sentences, no code fence. Stress the two template-level differences the reader should internalize: (1) `AddBlogSite` binds `AddMarkdownContent<BlogSiteFrontMatter>` (not `DocSiteFrontMatter`, not the core `BlogFrontMatter`), and (2) BlogSite's content-path defaults are `ContentRootPath = "Content"` + `BlogContentPath = "Blog"` serving at `BlogBaseUrl = "/blog"`, whereas DocSite drives URLs from `ContentArea` slugs under `ContentRootPath`. Everything else that diverges (hard-coded chrome, RSS-first layout, no area switcher) falls out of those two choices._

### Checkpoint — Services registered, middleware not yet mounted

- `dotnet build` succeeds
- `dotnet run` starts the host, but `/` still returns whatever the pre-BlogSite pipeline produced — BlogSite services are in DI but the middleware and endpoints are not mounted yet

---

## 3. Mount `UseBlogSite` and swap `RunBlogSiteAsync`

_One sentence: `UseBlogSite` is the middleware counterpart to `AddBlogSite` — one call mounts antiforgery, static files, Razor-component routing (`Home`, `Archive`, `Blog`, `Tag`, `Tags` live inside `Pennington.BlogSite.dll`), MonorailCSS, and core Pennington middleware in the right order; when `EnableRss` is true (the default) it also maps `/rss.xml`._

### Step 3.1 — Call `UseBlogSite` after `Build()`

_Show the signature. Emphasize that this single call replaces both the old `UsePennington` line and the hand-written `MapGet` fallback from stage 1 — the BlogSite Razor components now own `/`, `/archive`, `/blog/{*fileName}`, `/tags`, `/tags/{TagEncodedName}` (plus the `/topics` aliases), and `BlogContentResolver` handles per-request rendering._

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)
```

### Step 3.2 — Swap `RunAsync` for `RunBlogSiteAsync`

_`RunBlogSiteAsync` is a thin delegate to `RunOrBuildAsync`, so the same host serves live in dev and generates static HTML when invoked as `dotnet run -- build <baseUrl> <outputDir>`. Both positional args are optional (defaults: `/` and `output`). Don't explain the unified dev-vs-build invariant here — link to the dev-vs-build explanation page if you need to reference it._

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.RunBlogSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

### Step 3.3 — See the fully-wired host

_Show stage 2 — this is the canonical final shape and it matches `Program.cs` verbatim. Three calls: `AddBlogSite`, `UseBlogSite`, `RunBlogSiteAsync`. Compare the compact ~20-line body to the ~40-line stage-1 body and let the diff speak for itself — no prose justification._

```csharp:xmldocid,bodyonly
M:BlogSiteScaffoldExample.Stage2.Run(System.String[])
```

### Checkpoint — Full chrome renders

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see the BlogSite home layout: site title "Scaffold Blog", a recent-posts list with one entry, header chrome, and MonorailCSS styling

---

## 4. Drop in a placeholder post and verify every built-in route

_One sentence: posts live under `{ContentRootPath}/{BlogContentPath}` — with the defaults, that is `Content/Blog/`. A single placeholder keeps the home listing, archive, and RSS feed non-empty until the next tutorial teaches the full `BlogSiteFrontMatter` surface._

### Step 4.1 — Create `Content/Blog/hello-world.md`

_Show the placeholder post verbatim. Flag the four front-matter keys it uses (`title`, `description`, `date`, `author`) as the bare minimum the home listing and RSS feed consume — the next tutorial expands this to the full `BlogSiteFrontMatter` surface (tags, series, repository, section, redirectUrl)._

```text:path
examples/BlogSiteScaffoldExample/Content/Blog/hello-world.md
```

### Step 4.2 — Walk the built-in routes

_List the URLs and what each should render. Keep it mechanical; no rationale. The reader visits each in order and sees the placeholder surface on every page:_

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

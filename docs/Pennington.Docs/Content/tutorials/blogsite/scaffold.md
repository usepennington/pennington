---
title: "Scaffold a blog with BlogSite"
description: "Swap the bare Pennington host for the BlogSite template and configure the core options that drive the home, archive, post, tag, and RSS routes."
sectionLabel: Getting Started with BlogSite
order: 1
tags: [blogsite, template, scaffold, options]
uid: tutorials.blogsite.scaffold
---

By the end of this tutorial, a running BlogSite host titled "Scaffold Blog" serves a home listing, `/archive`, `/blog/<slug>/`, `/tags/`, `/tags/<name>/` (plus the `/topics` aliases), and `/rss.xml` — all from a single placeholder post under `Content/Blog/`.

`AddBlogSite` is a shortcut, the same kind `AddDocSite` is. It pre-wires what the [getting-started tutorials](xref:tutorials.getting-started.first-site) assemble by hand — host, layout, navigation, styling — into one call, tuned for a single shape: a site where the blog *is* the site. Reach for it when that shape fits; build on `AddPennington` directly for anything else. [What the templates wire for you](xref:explanation.positioning.docsite-positioning) covers what each call assembles and where the wiring stops.

## Prerequisites

BlogSite is a separate template — no DocSite experience needed.

> [!NOTE]
> DocSite and BlogSite can't run in the same app — pick one. BlogSite is the
> right choice when the blog *is* the site. If you mainly want documentation with
> a blog attached, use DocSite's native blog instead.

- .NET 11 SDK installed
- Completed [Create your first Pennington site](xref:tutorials.getting-started.first-site)
- Completed [Serve markdown through a Blazor catch-all](xref:tutorials.getting-started.first-page) (so `Content/` already has at least one markdown file)

The finished code for this tutorial lives in [`examples/BlogSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteScaffoldExample).

---

## 1. Start from the bare Pennington host

The host you built in the getting-started tutorials calls `AddPennington`, registers content with `AddMarkdownContent<DocFrontMatter>`, mounts `UsePennington`, and wires a hand-written `MapGet` fallback that walks `IContentService` to serve individual pages.

```csharp:symbol,bodyonly
examples/BlogSiteScaffoldExample/Stage1_BeforeAddBlogSite.cs > Stage1.Run
```

The three moving parts are the DI registration, the `UsePennington` call, and the hand-rolled `MapGet`. Absent: the home listing, `/archive`, `/blog/<slug>` pages, `/tags` and `/topics` aliases, the `/rss.xml` feed, and the MonorailCSS chrome. The next section brings all of that in with a single `AddBlogSite` call.

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/blog/hello-world` — the page shows unstyled HTML for the markdown. None of the BlogSite chrome (home listing, archive, tag pages, RSS) exists yet.

</Checkpoint>

---

## 2. Wire `AddBlogSite`, `UseBlogSite`, and `RunBlogSiteAsync`

`AddBlogSite` is the BlogSite template's single registration call; it stands in for `AddPennington` plus the `AddMarkdownContent<DocFrontMatter>` line, wiring Pennington core, MonorailCSS, the Razor chrome, and the blog content services in one call. `UseBlogSite` mounts the middleware stack and Razor component routes; `RunBlogSiteAsync` dispatches between dev-serve and static-build.

<Steps>
<Step StepNumber="1">

**Replace `Program.cs` with the BlogSite calls**

```csharp:symbol,bodyonly
examples/BlogSiteScaffoldExample/Stage3_UseBlogSite.cs > Stage3.Run
```

The options populated here cover site identity (`SiteTitle`, `Description`, `CanonicalBaseUrl`), content paths shown at their defaults (`ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`), and author fallbacks (`AuthorName`, `AuthorBio`). The full surface lives in <xref:reference.api.blog-site-options>.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- The BlogSite home layout appears: site title "Scaffold Blog", a recent-posts list with one entry, header chrome, and MonorailCSS styling

</Checkpoint>

---

## 3. Drop in a placeholder post and verify every built-in route

Posts live under `{ContentRootPath}/{BlogContentPath}` — with the defaults from step 2, that is `Content/Blog/`. A single placeholder post here keeps the home listing, archive, and RSS feed non-empty until the next tutorial introduces the full `BlogSiteFrontMatter` surface.

<Steps>
<Step StepNumber="1">

**Create `Content/Blog/hello-world.md`**

The placeholder post uses four front-matter keys: `title`, `description`, `date`, and `author`. These are the minimum the home listing and RSS feed need to render an entry. The next tutorial expands this to the full `BlogSiteFrontMatter` surface, adding `tags`, `series`, `repository`, `sectionLabel`, and `redirectUrl`.

```markdown:symbol
examples/BlogSiteScaffoldExample/Content/Blog/hello-world.md
```

</Step>
<Step StepNumber="2">

**Visit the home listing and the RSS feed**

- `/` shows the home listing with `hello-world` as the only recent post.
- `/rss.xml` returns RSS 2.0 with one `<item>` carrying the post title, link, description, pub date, and author.

The full route surface (`/archive`, `/blog/<slug>`, `/tags`, `/topics` aliases, `/rss.xml`) is catalogued in <xref:reference.blogsite.routes>.

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
- BlogSite binds posts through `AddMarkdownContent<BlogSiteFrontMatter>` (introduced in the next tutorial) and defaults content paths to `Content/Blog` served at `/blog`, which distinguishes it from the `DocSite` template's area-driven layout.
- Every built-in route the template ships responds: `/`, `/archive`, `/blog/<slug>`, `/tags` (and `/topics` aliases), `/tags/<name>`, and `/rss.xml`.

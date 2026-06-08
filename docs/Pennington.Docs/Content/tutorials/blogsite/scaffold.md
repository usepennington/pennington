---
title: "Scaffold a blog with BlogSite"
description: "Swap the bare Pennington host for the BlogSite template and configure the core options that drive the home, archive, post, tag, and RSS routes."
sectionLabel: Getting Started with BlogSite
order: 1
tags: [blogsite, template, scaffold, options]
uid: tutorials.blogsite.scaffold
---

By the end of this tutorial, a running BlogSite host titled "Scaffold Blog" serves a home listing, `/archive`, `/blog/<slug>/`, `/tags/`, `/tags/<name>/` (plus the `/topics` aliases), and `/rss.xml` — all from a single placeholder post under `Content/Blog/`.

Like `AddDocSite`, `AddBlogSite` is a packaged starting point. The [getting-started tutorials](xref:tutorials.getting-started.first-site) build up the host, layout, navigation, and styling step by step; this one call folds all of that into a single line, configured for a site where the blog *is* the site. If that matches what you're building, start here. If it doesn't, you keep more control by composing on top of `AddPennington` yourself. For a breakdown of what each call assembles and where its automation ends, see [What the templates wire for you](xref:explanation.positioning.docsite-positioning).

## Prerequisites

BlogSite is a separate template — no DocSite experience needed.

> [!NOTE]
> DocSite and BlogSite can't run in the same app — pick one. BlogSite is the
> right choice when the blog *is* the site. If you mainly want documentation with
> a blog attached, use DocSite's native blog instead.

- .NET 10 SDK installed
- Completed [Create your first Pennington site](xref:tutorials.getting-started.first-site)
- Completed [Serve markdown through a Blazor catch-all](xref:tutorials.getting-started.first-page) (so `Content/` already has at least one markdown file)

The stable .NET 10 SDK is all BlogSite needs — its package targets .NET 10, and you never write the `union` keyword that would call for a preview SDK. See [the SDK and the union shim](xref:explanation.positioning.sdk-and-the-union-shim) for when the .NET 11 beta is worth opting into.

The finished code for this tutorial lives in [`examples/BlogSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteScaffoldExample).

---

## 1. Start from the bare Pennington host

The host you built in the getting-started tutorials calls `AddPennington`, registers content with `AddMarkdownContent<DocFrontMatter>`, mounts `UsePennington`, and wires a hand-written `MapGet` fallback that walks `IContentService` to serve individual pages.

```csharp:symbol,bodyonly
examples/BlogSiteScaffoldExample/Stage1_BeforeAddBlogSite.cs > Stage1.Run
```

The three moving parts are the DI registration, the `UsePennington` call, and the hand-rolled `MapGet`. Absent: the home listing, `/archive`, `/blog/<slug>` pages, `/tags` and `/topics` aliases, the `/rss.xml` feed, and the [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) chrome. The next section brings all of that in with a single `AddBlogSite` call.

**Add the first post**

Posts live under `{ContentRootPath}/{BlogContentPath}` — `Content/Blog/` with the defaults. Add one placeholder post so the host has something to serve. It uses four front-matter keys — `title`, `description`, `date`, and `author` — the minimum the home listing and RSS feed will need once the template is wired:

```markdown:symbol
examples/BlogSiteScaffoldExample/Content/Blog/hello-world.md
```

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

The options populated here cover site identity (`SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`), content paths shown at their defaults (`ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`), and author fallbacks (`AuthorName`, `AuthorBio`). The full surface lives in <xref:reference.api.blog-site-options>.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- The BlogSite home layout appears: site title "Scaffold Blog", a recent-posts list with one entry, header chrome, and MonorailCSS styling

</Checkpoint>

---

## 3. Verify every built-in route

With the post from section 1 in place and `AddBlogSite` wired, every route the template ships now responds. The next tutorial expands the post to the full `BlogSiteFrontMatter` surface, adding `tags`, `series`, `repository`, `sectionLabel`, and `redirectUrl`.

**Visit the home listing and the RSS feed**

- `/` shows the home listing with `hello-world` as the only recent post.
- `/rss.xml` returns RSS 2.0 with one `<item>` carrying the post title, link, description, pub date, and author.

The full route surface (`/archive`, `/blog/<slug>`, `/tags`, `/topics` aliases, `/rss.xml`) is cataloged in <xref:reference.blogsite.routes>.

<Checkpoint>

- Each URL above returns 200 and renders the placeholder post's metadata
- `/rss.xml` returns `application/rss+xml` content with one item whose `<guid>` matches the canonical post URL

</Checkpoint>

---

## Summary

- The bare `AddPennington` host gave way to `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`, and the full BlogSite chrome now renders.
- The core `BlogSiteOptions` surface — `SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`, `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`, `AuthorName`, `AuthorBio` — is populated, and each field flows through to the rendered output.
- BlogSite binds posts through `AddMarkdownContent<BlogSiteFrontMatter>` (introduced in the next tutorial) and defaults content paths to `Content/Blog` served at `/blog`, which distinguishes it from the `DocSite` template's area-driven layout.
- Every built-in route the template ships responds: `/`, `/archive`, `/blog/<slug>`, `/tags` (and `/topics` aliases), `/tags/<name>`, and `/rss.xml`.

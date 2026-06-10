---
title: "Scaffold a blog with BlogSite"
description: "Swap the bare Pennington host for the BlogSite template and configure the core options that drive the home, archive, post, tag, and RSS routes."
sectionLabel: Getting Started with BlogSite
order: 1
tags: [blogsite, template, scaffold, options]
uid: tutorials.blogsite.scaffold
---

By the end of this tutorial, a running BlogSite host titled "Scaffold Blog" serves a home listing, `/archive`, `/blog/<slug>/`, `/tags/`, `/tags/<name>/`, and `/rss.xml` — all from a single placeholder post under `Content/Blog/`.

`AddBlogSite` folds the host, layout, navigation, and styling into one call, configured for a site where the blog *is* the site; for what the template wires, where the wiring stops, and why DocSite and BlogSite can't share an app, read [what the templates wire for you](xref:explanation.positioning.docsite-positioning) first.

## Prerequisites

- .NET 10 SDK installed
- Completed [Create your first Pennington site](xref:tutorials.getting-started.first-site)
- Completed [Serve markdown through a Blazor catch-all](xref:tutorials.getting-started.first-page) (so `Content/` already has at least one markdown file)

The stable .NET 10 SDK is all BlogSite needs — its package targets .NET 10, and you never write the `union` keyword that would call for a preview SDK. See [the SDK and the union shim](xref:explanation.positioning.sdk-and-the-union-shim) for when the .NET 11 beta is worth opting into.

The finished code for this tutorial lives in [`examples/BlogSiteScaffoldExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteScaffoldExample).

---

## 1. Start from the bare Pennington host

The host you built in the getting-started tutorials calls `AddPennington`, registers content with `AddMarkdownContent<DocFrontMatter>`, mounts `UsePennington`, and routes through a Blazor `@page "/{*Path}"` catch-all (`MarkdownPage.razor`) that resolves each URL with `IPageResolver` to serve individual pages.

That host serves individual pages but nothing else: no home listing, no `/archive`, no `/blog/<slug>` pages, no `/tags` listings, no `/rss.xml` feed, and no [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) chrome. The next section brings all of that in with a single `AddBlogSite` call.

<Steps>
<Step StepNumber="1">

**Add the first post**

Posts live under `{ContentRootPath}/{BlogContentPath}` — `Content/Blog/` with the defaults. Add one placeholder post so the host has something to serve. It uses four front-matter keys — `title`, `description`, `date`, and `author` — the minimum the home listing and RSS feed will need once the template is wired:

```markdown:symbol
examples/BlogSiteScaffoldExample/Content/Blog/hello-world.md
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `/blog/hello-world` at the URL the console prints — the page shows unstyled HTML for the markdown. None of the BlogSite chrome (home listing, archive, tag pages, RSS) exists yet.

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

The options populated here cover site identity (`SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`), content paths shown at their defaults (`ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`), and author fallbacks (`AuthorName`, `AuthorBio`). The full surface lives in <xref:reference.api.blog-site-options>.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `/` at the URL the console prints
- The BlogSite home layout appears: site title "Scaffold Blog", a recent-posts list with one entry, header chrome, and MonorailCSS styling

</Checkpoint>

---

## 3. Verify every built-in route

With the post from section 1 in place and `AddBlogSite` wired, every route the template ships now responds. The placeholder post carries `tags: [scaffold]`, so the tag routes have one entry to list.

<Steps>
<Step StepNumber="1">

**Walk the page routes**

With the host running, visit each of these at the URL the console prints. Every one returns 200:

- `/` — home listing with `hello-world` as the only recent post.
- `/archive` — full archive, same single post in reverse-chronological order.
- `/blog/hello-world` — the post itself, now rendered with BlogSite chrome.
- `/tags` — the tag index, showing `scaffold` with a count of one.
- `/tags/scaffold` — the per-tag listing with the one post.

</Step>
<Step StepNumber="2">

**Check the RSS feed**

Visit `/rss.xml`. It returns `application/rss+xml` with one `<item>` carrying the post title, link, description, pub date, and author.

</Step>
</Steps>

The full route surface, including the paginated `/archive/page/{n}` and per-tag pages, is cataloged in <xref:reference.blogsite.routes>. The next tutorial expands the post to the full `BlogSiteFrontMatter` surface, adding `tags`, `series`, `repository`, `sectionLabel`, and `redirectUrl`.

<Checkpoint>

- Each page route above returns 200 and renders the placeholder post's metadata
- `/rss.xml` returns `application/rss+xml` content with one item whose `<guid>` matches the canonical post URL

</Checkpoint>

---

## Summary

- The bare `AddPennington` host gave way to `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`, and the full BlogSite chrome now renders.
- The core `BlogSiteOptions` surface — `SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`, `ContentRootPath`, `BlogContentPath`, `BlogBaseUrl`, `AuthorName`, `AuthorBio` — is populated, and each field flows through to the rendered output.
- BlogSite binds posts through `AddMarkdownContent<BlogSiteFrontMatter>` (introduced in the next tutorial) and defaults content paths to `Content/Blog` served at `/blog`, which distinguishes it from the `DocSite` template's area-driven layout.
- Every built-in route the template ships responds: `/`, `/archive`, `/blog/<slug>`, `/tags`, `/tags/<name>`, and `/rss.xml`.

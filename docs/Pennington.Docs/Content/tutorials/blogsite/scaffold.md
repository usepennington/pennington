---
title: "Scaffold a blog with BlogSite"
description: "Swap the core Pennington wiring for the BlogSite template and configure its required BlogSiteOptions."
section: "blogsite"
order: 10
tags: []
uid: tutorials.blogsite.scaffold
isDraft: true
search: false
llms: false
---

> **In this page.** Replacing `AddPennington` with `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync`, configuring the core `BlogSiteOptions` (site title, author, canonical base URL, content paths), and understanding the difference between `DocSite` and `BlogSite` defaults.
>
> **Not in this page.** Authoring individual posts (next page) or customizing the homepage hero (see the final page in this section).

## What you'll do

- Outline bullet: Artifact — a running BlogSite at `http://localhost:5000/` that shows the default hero, a blog index at `/blog`, and a tags index at `/tags`, populated by one placeholder post.
- Outline bullet: Skill — you'll know how to swap `AddPennington` for `AddBlogSite`, fill in the required `BlogSiteOptions` fields, and recognize which defaults differ from the DocSite template.

## Prerequisites

- Outline bullet: .NET 11 SDK installed.
- Outline bullet: Completed the "Getting Started with Pennington" tutorials through "Create your first Pennington site" (`/tutorials/getting-started/first-site`) — you should already have a working Pennington project with `AddPennington` + `UsePennington`.
- Outline bullet: A terminal open at the root of that project and the project already runs under `dotnet run`.
- Outline bullet: Finished-code pointer — this tutorial's end state matches [`examples/AlexBlogExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/AlexBlogExample).

---

## 1. Swap the wiring from AddPennington to AddBlogSite

One sentence: change the three host-wiring calls so the BlogSite template composes its own services, middleware, and CLI handling around the core Pennington engine.

### Step 1.1 — Replace service registration with `AddBlogSite`

- Outline bullet: Open `Program.cs` and delete the `builder.Services.AddPennington(...)` call.
- Outline bullet: Replace it with `builder.Services.AddBlogSite(() => new BlogSiteOptions { ... })`, noting the factory signature `Func<BlogSiteOptions>` (verified in `src/Pennington.BlogSite/BlogSiteServiceExtensions.cs`).
- Outline bullet: Add `using Pennington.BlogSite;` at the top — `BlogSiteOptions`, `HeroContent`, `SocialLink`, `Project`, and `HeaderLink` all live in that namespace.
- Outline bullet: Note that `AddBlogSite` internally composes `AddPennington` + `AddMonorailCss` plus the file-watched `BlogContentResolver` and `BlogSiteContentService` — you do not register those yourself.

```csharp raw-file="examples/AlexBlogExample/Program.cs"
```

- Outline bullet: Snippet source — `examples/AlexBlogExample/Program.cs` is the canonical minimal BlogSite wiring in the examples inventory; it shows every change in context. (Raw-file fence chosen because `Program.cs` is top-level statements with no xmldocid-addressable symbol.)

### Step 1.2 — Replace the middleware call with `UseBlogSite`

- Outline bullet: Replace `app.UsePennington()` with `app.UseBlogSite()`.
- Outline bullet: Explain that `UseBlogSite` wires antiforgery, static files, `MapRazorComponents<App>`, MonorailCSS, and then `UsePennington` internally — you do not call `UsePennington` yourself.
- Outline bullet: Keep the `app.MapStaticAssets()` call that Pennington-based hosts already use.

### Step 1.3 — Replace the run call with `RunBlogSiteAsync`

- Outline bullet: Replace `await app.RunOrBuildAsync(args)` with `await app.RunBlogSiteAsync(args)`.
- Outline bullet: Note that `RunBlogSiteAsync` is a thin delegate to `RunOrBuildAsync`, so `dotnet run` serves and `dotnet run -- build` produces static output exactly as in the core tutorials.

### Checkpoint — What you should see now

- Outline bullet: Run `dotnet run` and watch the console log "Now listening on ...".
- Outline bullet: Visit `http://localhost:5000/` and see the BlogSite homepage shell — it will still be mostly empty until we fill in the options in step 2.
- Outline bullet: Stop with `Ctrl+C`.

---

## 2. Fill in the required BlogSiteOptions fields

One sentence: `BlogSiteOptions` has two `required` properties and a cluster of commonly-set options you should fill in before the homepage is usable.

### Step 2.1 — Set `SiteTitle` and `Description`

- Outline bullet: These two properties are marked `required` on the `BlogSiteOptions` record (verified in `src/Pennington.BlogSite/BlogSiteOptions.cs`) — the compiler will refuse to build without them.
- Outline bullet: Pick a short site title (shows up in the `<title>` tag and header) and a one-sentence description (used in feed metadata and the default meta description).

### Step 2.2 — Set the author fields

- Outline bullet: Set `AuthorName` and `AuthorBio` on `BlogSiteOptions` — both are `string?` and feed the default author blurb in the homepage layout.
- Outline bullet: These appear alongside `HeroContent` on the homepage; the hero itself is out of scope for this tutorial and is covered in `/tutorials/blogsite/hero-projects-socials`.

### Step 2.3 — Set the canonical base URL

- Outline bullet: Set `CanonicalBaseUrl` (for example `"https://yourname.dev"`). This is the base URL baked into the RSS feed, sitemap, and `<link rel="canonical">` tags.
- Outline bullet: Leaving it null works in dev but produces incorrect absolute URLs in the built output.

### Step 2.4 — Confirm the content paths

- Outline bullet: `ContentRootPath` defaults to `"Content"` and `BlogContentPath` defaults to `"Blog"` — the tree Pennington reads will be `Content/Blog/` unless you change either.
- Outline bullet: `BlogBaseUrl` defaults to `/blog` and `TagsPageUrl` defaults to `/tags` — those are the URLs the blog index and the tags index mount at.
- Outline bullet: Create an empty `Content/Blog/` folder with one placeholder markdown file (e.g. `hello.md` with front matter `title: Hello`) so the blog index has at least one entry; authoring the file properly is the next tutorial.

### Checkpoint — What you should see now

- Outline bullet: Run `dotnet run` again.
- Outline bullet: Visit `http://localhost:5000/` — the `SiteTitle` appears in the browser tab, the author name appears in the layout, and the navigation links to `/blog`.
- Outline bullet: Visit `http://localhost:5000/blog` — the blog index lists the placeholder post.
- Outline bullet: Visit `http://localhost:5000/tags` — the tags index page renders (empty, because the placeholder has no tags yet).

---

## 3. Know what changed versus the DocSite defaults

One sentence: BlogSite and DocSite are both built on `AddPennington` but expose different option shapes and default URLs — this unit makes those differences concrete so you are not surprised when you reference DocSite tutorials.

### Step 3.1 — Compare the required fields

- Outline bullet: Both `BlogSiteOptions` (in `src/Pennington.BlogSite/BlogSiteOptions.cs`) and `DocSiteOptions` (in `src/Pennington.DocSite/DocSiteOptions.cs`) require `SiteTitle` and `Description`.
- Outline bullet: `DocSiteOptions` adds `SolutionPath` and `Areas` and `ConfigureLocalization`; `BlogSiteOptions` does not carry those.
- Outline bullet: `BlogSiteOptions` adds `BlogContentPath`, `BlogBaseUrl`, `TagsPageUrl`, `AuthorName`, `AuthorBio`, `HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`, and `SocialMediaImageUrlFactory`; `DocSiteOptions` does not carry those.
- Outline bullet: Both share `CanonicalBaseUrl`, `ColorScheme`, `ContentRootPath`, `ExtraStyles`, font options, `FontPreloads`, and `AdditionalRoutingAssemblies`.

### Step 3.2 — Compare the defaults and URLs

- Outline bullet: `ContentRootPath` default is `"Content"` in both; in BlogSite, posts live under `BlogContentPath` (default `"Blog"`) inside that root.
- Outline bullet: `EnableRss` and `EnableSitemap` default to `true` on `BlogSiteOptions` — the blog template wires `/rss.xml` automatically when `EnableRss` is true; DocSite has no equivalent option.
- Outline bullet: Static-file handling, MonorailCSS registration, and `MapRazorComponents<App>` are identical — both templates delegate to the core `UsePennington` middleware for the response-processor pipeline.

### Checkpoint — What you should see now

- Outline bullet: Visit `http://localhost:5000/rss.xml` — the RSS feed renders with the placeholder post (because `EnableRss` defaulted to `true`).
- Outline bullet: Visit `http://localhost:5000/sitemap.xml` — the sitemap lists the blog index, tags index, and the placeholder post URL.
- Outline bullet: Stop the host with `Ctrl+C`.

---

## Summary

- Outline bullet: You replaced `AddPennington` / `UsePennington` / `RunOrBuildAsync` with their BlogSite counterparts and saw the default blog homepage, blog index, and tags index materialize.
- Outline bullet: You filled in the required `BlogSiteOptions` fields (`SiteTitle`, `Description`) and the common author and canonical-URL fields.
- Outline bullet: You can now identify which options are BlogSite-specific versus shared with DocSite, so cross-referencing DocSite tutorials won't confuse you.
- Outline bullet: You have a placeholder post wired up — authoring it properly is the next tutorial.

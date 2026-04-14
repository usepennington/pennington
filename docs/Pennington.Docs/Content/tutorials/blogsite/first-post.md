---
title: "Author your first post with BlogSiteFrontMatter"
description: "Replace the scaffold's placeholder post with a fully-populated BlogSiteFrontMatter block and watch it flow into the blog index, per-tag pages, and the built-in RSS feed."
sectionLabel: "Getting Started with BlogSite"
order: 20
tags:
  - blogsite
  - front-matter
  - rss
  - authoring
uid: tutorials.blogsite.first-post
---

> **In this page.** _One sentence paraphrasing the Covers line: the reader writes a post backed by `BlogSiteFrontMatter` (title, description, date, author, tags, series, repository, section, redirectUrl), confirms it appears on the blog index and `/tags/<tag>/` listings, and turns on the built-in RSS feed by setting `EnableRss = true` on `BlogSiteOptions`. Be explicit that the template binds `AddMarkdownContent<BlogSiteFrontMatter>` — NOT the core `BlogFrontMatter` — so this is the record the tutorial teaches._
>
> **Not in this page.** _One sentence paraphrasing the Does-not-cover line: customizing the post template body is out of scope (point at the DocSite-components how-to which applies symmetrically), and the `/tags/<tag>/` index is surfaced but not customized here — a later tutorial and the BlogSite routes reference page cover the tag-index surface._

## What you'll do

_**Artifact** (one sentence): a running BlogSite at `http://localhost:5000` whose home page, archive, per-tag index, and `/rss.xml` all surface a single, fully-populated post — the placeholder from the scaffold tutorial is gone._

_**Skill** (one sentence): you'll know every `BlogSiteFrontMatter` field a post author ever touches, how each one lights up a different blog surface, and how to confirm RSS is on by opening `/rss.xml`._

## Prerequisites

_Keep this list short. The scaffold tutorial established the `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync` host. This tutorial does not re-register content services — it only edits a markdown file and sets one explicit flag. Don't explain the host shape again; link back to the scaffold tutorial._

- .NET 11 SDK installed
- Completed [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold) (or have that example's `Program.cs` and a single placeholder post ready to reuse)
- A code editor that renders YAML front matter cleanly (VS Code, Rider, etc.)

The finished code for this tutorial lives in [`examples/BlogSiteFirstPostExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteFirstPostExample).

---

## 1. Start from a bare-minimum front-matter block

_One sentence: the reader replaces the scaffold's placeholder post with a new file `Content/Blog/my-first-post.md` carrying only the three fields every post truly needs — `title`, `description`, `date` — and confirms it renders on the home listing, the archive, and the RSS feed even in this minimal state._

### Step 1.1 — Delete `Content/Blog/hello-world.md` and create `my-first-post.md`

_One sentence of setup: the scaffold tutorial left a placeholder post named `hello-world.md` in `Content/Blog/`. Have the reader delete it and drop a new file `my-first-post.md` in the same folder. Call out that the filename (minus `.md`) becomes the URL slug: the post will serve at `/blog/my-first-post/`. Do not explain the routing layer — this is the same file-path-to-URL mapping the Pennington first-page tutorial already covered._

### Step 1.2 — Paste in title, description, and date only

_One sentence: the reader pastes the Stage 1 markdown body into the new file. This is the smallest front matter that lets a BlogSite post render cleanly: `title` is the only field required by `IFrontMatter`; `description` is what the home card, archive card, and RSS `<description>` element all pull from; `date` drives both the archive sort and the RSS `<pubDate>` element._

```csharp:xmldocid,bodyonly
M:BlogSiteFirstPostExample.Stage1.Source
```

_Explain the two `---` fences and that `date:` is parsed as an ISO-8601 date. Don't get into capability interfaces — point curious readers at the front-matter-keys reference page if they ask._

### Checkpoint — The new post replaces the scaffold placeholder

_Concrete verification: the reader runs `dotnet run`, visits the home page, and sees exactly one post card (the new one), with the scaffold placeholder gone._

- Run `dotnet run` from the example project
- Visit `http://localhost:5000/` — you should see a single recent-posts card titled **Shipping a tiny content engine for weekend projects** with the stage-1 description
- Visit `http://localhost:5000/archive` — the same post appears as the only archive entry, dated **2026-04-10**
- Visit `http://localhost:5000/blog/my-first-post/` — the post body renders with its H1 and paragraph text

---

## 2. Populate every `BlogSiteFrontMatter` field

_One sentence: the reader expands the front-matter block to populate every `BlogSiteFrontMatter` field a post author ever touches — `author`, `tags`, `series`, `repository`, `sectionLabel`, and an empty `redirectUrl:` — and watches each one light up a different surface in the running site._

### Step 2.1 — Replace the front matter with the fully-populated block

_One sentence: have the reader replace the Stage 1 YAML block with the Stage 2 block below. Walk through each new key as it lands — `author:` becomes the byline and the RSS `<author>` element; `tags:` builds the `/tags/<tag>/` index pages; `series:` threads posts together under a shared banner on the post chrome; `repository:` renders as a "Source Code" link card on the post page; `sectionLabel:` groups the post under a named slice of the archive; `redirectUrl:` stays empty because this post has no previous home on the web._

```csharp:xmldocid,bodyonly
M:BlogSiteFirstPostExample.Stage2.Source
```

_Explain that the list-of-strings shape for `tags:` is YAML's block sequence and that each tag renders as both a chip on the post page and its own index at `/tags/<tag>/`. Point at `T:Pennington.BlogSite.BlogSiteFrontMatter` if the reader wants to see the record definition._

### Step 2.2 — Reload and confirm every surface lit up

_One sentence: the reader reloads the running site and verifies each new field in turn — the byline on the post page, the tag chips, the source-code link card, the series banner, and the `/tags/<tag>/` listings. No code was edited; the host from the scaffold tutorial is untouched._

### Checkpoint — Each field has a visible home

_Concrete verification by surface — reader walks each URL and confirms what shows up._

- Visit `http://localhost:5000/blog/my-first-post/` — the post header now shows the byline **Author Name**, the series banner **Pennington Field Notes**, three tag chips (**pennington**, **dotnet**, **blogging**), and a **Source Code** link card pointing at the `repository:` URL
- Visit `http://localhost:5000/tags/pennington/` — the post appears on the per-tag index; repeat for `/tags/dotnet/` and `/tags/blogging/`
- Visit `http://localhost:5000/archive` — the archive card now shows the longer description from the Stage 2 block

---

## 3. Turn on the built-in RSS feed

_One sentence: the reader makes the RSS wiring explicit by setting `EnableRss = true` on `BlogSiteOptions` — it already defaults to `true`, so this step is about giving the reader a concrete symbol to point at, visiting `/rss.xml`, and confirming the populated front matter reached the feed items._

### Step 3.1 — Set `EnableRss = true` explicitly in `Program.cs`

_One sentence: have the reader open `Program.cs` from the scaffold tutorial and add one explicit line inside the `AddBlogSite(...)` block: `EnableRss = true,`. Call out in one sentence that this is the default (see `P:Pennington.BlogSite.BlogSiteOptions.EnableRss`) — the reason to set it explicitly is so the line exists in source when the reader later flips it off, and so this tutorial has a concrete symbol to point at. Also confirm `CanonicalBaseUrl` is set (it already is from the scaffold) because the RSS feed uses it to build absolute `<link>` elements._

```csharp:path
examples/BlogSiteFirstPostExample/Program.cs
```

_Explain that `UseBlogSite()` maps `/rss.xml` when `EnableRss` is true (see `M:Pennington.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)`). The reader did not write the feed-building code — the template did it — but the populated front matter is what makes the feed items interesting._

### Step 3.2 — Open `/rss.xml` and confirm the post is an entry

_One sentence: the reader visits `http://localhost:5000/rss.xml` in the browser (or `curl`s it) and confirms that the post appears as a `<item>` with every front-matter field mapped to its RSS element: `title:` → `<title>`, `description:` → `<description>`, `date:` → `<pubDate>`, `author:` → `<author>`, and the canonical post URL → `<link>` and `<guid>`._

### Checkpoint — A valid RSS feed with the populated post

_Concrete verification: the reader opens the feed URL and sees one `<item>` element with every expected child._

- Visit `http://localhost:5000/rss.xml` — the browser either renders the feed (Firefox) or shows raw XML (Chrome/Edge)
- Confirm the `<channel>` carries the site title **First Post Blog** and the configured description
- Confirm a single `<item>` element contains `<title>Shipping a tiny content engine for weekend projects</title>`, `<description>` with the stage-2 text, `<pubDate>` for **10 Apr 2026**, `<author>Author Name</author>`, and a `<link>` whose value starts with the configured `CanonicalBaseUrl`

---

## Summary

_Three to five bullets. Each bullet names a capability the reader now has — not a topic covered._

- You can author a Pennington blog post backed by `BlogSiteFrontMatter` and predict which blog surface each field drives — title/description/date for listings, author for byline and RSS, tags for `/tags/<tag>/` indexes, series for the shared banner, repository for the source-code link card.
- You know that `AddBlogSite` binds `AddMarkdownContent<BlogSiteFrontMatter>` — not the core `BlogFrontMatter` — so the YAML keys your posts accept are the ones on `BlogSiteFrontMatter`.
- You can turn on (or off) the built-in RSS feed with `EnableRss` on `BlogSiteOptions`, and you've seen how populated front matter flows into every RSS item element.
- You can drop a new `Content/Blog/*.md` file and watch it appear on the home page, the archive, every tag it claims, and `/rss.xml` without touching `Program.cs` again.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.

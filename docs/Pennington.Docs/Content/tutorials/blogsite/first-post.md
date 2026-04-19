---
title: "Author your first post with BlogSiteFrontMatter"
description: "Replace the scaffold's placeholder post with a fully-populated BlogSiteFrontMatter block and watch it flow into the blog index, per-tag pages, and the built-in RSS feed."
sectionLabel: "Getting Started with BlogSite"
order: 103020
tags:
  - blogsite
  - front-matter
  - rss
  - authoring
uid: tutorials.blogsite.first-post
---

By the end of this tutorial, a running BlogSite at `http://localhost:5000` surfaces a single, fully-populated post on the home page, the archive, the per-tag index, and `/rss.xml` — the placeholder from the scaffold tutorial swapped for a post of your own. Along the way, every `BlogSiteFrontMatter` field a post author touches comes into view, along with the surface each one lights up and how to confirm RSS is on by opening `/rss.xml`.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold) (or have that example's `Program.cs` and a single placeholder post ready to reuse)
- A code editor that renders YAML front matter cleanly (VS Code, Rider, etc.)

The finished code for this tutorial lives in [`examples/BlogSiteFirstPostExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteFirstPostExample).

---

## 1. Start from a bare-minimum front-matter block

This section replaces the scaffold's placeholder post with a new file `Content/Blog/my-first-post.md` that carries only the three fields every post truly needs — `title`, `description`, and `date` — then confirms it renders on the home listing, the archive, and the RSS feed even in this minimal state.

<Steps>
<Step StepNumber="1">

**Delete `Content/Blog/hello-world.md` and create `my-first-post.md`**

The scaffold tutorial left a placeholder post named `hello-world.md` in `Content/Blog/`. Delete it, then create a new file `my-first-post.md` in the same folder. The filename (minus `.md`) becomes the URL slug, so the post serves at `/blog/my-first-post/`.

</Step>
<Step StepNumber="2">

**Paste in title, description, and date only**

Paste the Stage 1 markdown body into the new file. These three fields are the smallest front matter that lets a BlogSite post render cleanly: `title` is the only field required by `IFrontMatter`; `description` is what the home card, archive card, and RSS `<description>` element all pull from; and `date` drives both the archive sort order and the RSS `<pubDate>` element.

```markdown:path
examples/BlogSiteFirstPostExample/snippets/stage1.md
```

The two `---` fences delimit the YAML front matter block. The `date:` value parses as an ISO-8601 date; any format that round-trips as a date string works. For the full list of recognised front-matter keys, see the <xref:reference.api.blog-front-matter> reference page.

</Step>
</Steps>

### Checkpoint — The new post replaces the scaffold placeholder

- Run `dotnet run` from the example project
- Visit `http://localhost:5000/` — a single recent-posts card titled **Shipping a tiny content engine for weekend projects** appears with the stage-1 description
- Visit `http://localhost:5000/archive` — the same post appears as the only archive entry, dated **2026-04-10**
- Visit `http://localhost:5000/blog/my-first-post/` — the post body renders with its H1 and paragraph text

---

## 2. Populate every `BlogSiteFrontMatter` field

Next, the front-matter block expands to cover every `BlogSiteFrontMatter` field a post author touches — `author`, `tags`, `series`, `repository`, `sectionLabel`, and `redirectUrl` — and each one lights up a different surface in the running site.

<Steps>
<Step StepNumber="1">

**Replace the front matter with the fully-populated block**

Replace the Stage 1 YAML block with the Stage 2 block below. Here's what each new key does:

- `author:` — becomes the byline on the post page and the `<author>` element in the RSS feed
- `tags:` — builds `/tags/<tag>/` index pages and renders as chips on the post page
- `series:` — threads posts together under a shared banner on the post chrome
- `repository:` — renders as a "Source Code" link card on the post page
- `sectionLabel:` — groups the post under a named slice of the archive
- `redirectUrl:` — stays empty here because this post has no previous home on the web; set it when migrating a post from another URL

```markdown:path
examples/BlogSiteFirstPostExample/snippets/stage2.md
```

The list-of-strings shape for `tags:` is YAML's block sequence (`- value` per line). For the full record definition, see <xref:reference.api.blog-front-matter>.

</Step>
<Step StepNumber="2">

**Reload and confirm every surface lit up**

With the file saved, reload the running site and verify each new field in turn. No code changes are needed — the host from the scaffold tutorial stays untouched.

</Step>
</Steps>

### Checkpoint — Each field has a visible home

- Visit `http://localhost:5000/blog/my-first-post/` — the post header shows the byline **Author Name**, the series banner **Pennington Field Notes**, three tag chips (**pennington**, **dotnet**, **blogging**), and a **Source Code** link card pointing at the `repository:` URL
- Visit `http://localhost:5000/tags/pennington/` — the post appears on the per-tag index; repeat for `/tags/dotnet/` and `/tags/blogging/`
- Visit `http://localhost:5000/archive` — the archive card carries the longer description from the Stage 2 block

---

## 3. Turn on the built-in RSS feed

Now to make the RSS wiring explicit. `EnableRss` already defaults to `true`, but putting the line in source gives a concrete symbol to flip when turning the feed off, and confirms that the populated front matter reached the feed items.

<Steps>
<Step StepNumber="1">

**Set `EnableRss = true` explicitly in `Program.cs`**

Open `Program.cs` from the scaffold tutorial and add one explicit line inside the `AddBlogSite(...)` block: `EnableRss = true,`. This mirrors the default (see the `EnableRss` row in [`BlogSiteOptions`](xref:reference.api.blog-site-options)) but makes the intent clear. Also confirm `CanonicalBaseUrl` is set — it already is from the scaffold — because the RSS feed uses it to build absolute `<link>` elements.

```csharp:path
examples/BlogSiteFirstPostExample/Program.cs
```

`UseBlogSite()` maps the `/rss.xml` route when `EnableRss` is `true` (see [Built-in BlogSite routes](xref:reference.blogsite.routes)). The template builds the feed — populated front matter is what makes each feed item meaningful.

</Step>
<Step StepNumber="2">

**Open `/rss.xml` and confirm the post is an entry**

Visit `http://localhost:5000/rss.xml` in the browser (or `curl` it). Confirm the post appears as an `<item>` with every front-matter field mapped to its RSS element: `title:` → `<title>`, `description:` → `<description>`, `date:` → `<pubDate>`, `author:` → `<author>`, and the canonical post URL → `<link>` and `<guid>`.

</Step>
</Steps>

### Checkpoint — A valid RSS feed with the populated post

- Visit `http://localhost:5000/rss.xml` — the browser either renders the feed (Firefox) or shows raw XML (Chrome/Edge)
- The `<channel>` carries the site title **First Post Blog** and the configured description
- A single `<item>` element contains `<title>Shipping a tiny content engine for weekend projects</title>`, `<description>` with the stage-2 text, `<pubDate>` for **10 Apr 2026**, `<author>Author Name</author>`, and a `<link>` whose value starts with the configured `CanonicalBaseUrl`

---

## Summary

- A Pennington blog post backed by `BlogSiteFrontMatter` maps predictably onto each blog surface — title/description/date for listings, author for byline and RSS, tags for `/tags/<tag>/` indexes, series for the shared banner, repository for the source-code link card.
- `AddBlogSite` binds `AddMarkdownContent<BlogSiteFrontMatter>` — not the core `BlogFrontMatter` — so the YAML keys a post accepts are the ones on `BlogSiteFrontMatter`.
- The `EnableRss` option on `BlogSiteOptions` turns the built-in RSS feed on or off, and populated front matter flows into every RSS item element.
- Dropping a new `Content/Blog/*.md` file brings it straight to the home page, the archive, every tag it claims, and `/rss.xml` — no `Program.cs` changes needed.

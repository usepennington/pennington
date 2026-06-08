---
title: "Publish your first post and light up the RSS feed"
description: "Replace the scaffold's placeholder post with a fully-populated BlogSiteFrontMatter block and watch it flow into the blog index, per-tag pages, and the built-in RSS feed."
sectionLabel: "Getting Started with BlogSite"
order: 2
tags:
  - blogsite
  - front-matter
  - rss
  - authoring
uid: tutorials.blogsite.first-post
---

By the end of this tutorial, a running BlogSite at `http://localhost:5000` surfaces a single, fully-populated post on the home page, the archive, the per-tag index, and `/rss.xml` — the placeholder from the scaffold tutorial swapped for a post of your own. Each `BlogSiteFrontMatter` field maps to a specific surface; the steps walk through them in turn.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold) (or have that example's `Program.cs` and a single placeholder post ready to reuse)
- A code editor that renders YAML front matter cleanly (VS Code, Rider, and so on)

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

Paste the markdown body below into the new file. These three fields are the smallest front matter that lets a BlogSite post render cleanly: `title` is the post's heading and link label; `description` is what the home card, archive card, and RSS `<description>` element all pull from; and `date` drives both the archive sort order and the RSS `<pubDate>` element.

```markdown:symbol
examples/BlogSiteFirstPostExample/snippets/stage1.md
```

The two `---` fences delimit the YAML front matter block. The `date:` value parses as an ISO-8601 date; any format that round-trips as a date string works. For the full list of recognized front-matter keys, see the <xref:reference.api.blog-site-front-matter> reference page.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` from the example project
- Visit `http://localhost:5000/` — a single recent-posts card titled **Shipping a tiny content engine for weekend projects** appears with the description from the front matter
- Visit `http://localhost:5000/archive` — the same post appears as the only archive entry, dated **2026-04-10**
- Visit `http://localhost:5000/blog/my-first-post/` — the post body renders with its H1 and paragraph text

</Checkpoint>

---

## 2. Populate every `BlogSiteFrontMatter` field

Next, the front-matter block expands to cover every `BlogSiteFrontMatter` field a post author touches — `author`, `tags`, `series`, `repository`, `sectionLabel`, and `redirectUrl` — and each one drives a different surface in the running site.

<Steps>
<Step StepNumber="1">

**Replace the front matter with the fully-populated block**

Replace the existing YAML block with the fully-populated block below. Each new key drives a different surface:

- `author:` — byline on the post page and the `<author>` element in the RSS feed
- `tags:` — `/tags/<tag>/` index pages plus chips on the post page
- `series:` — shared-banner threading on the post chrome
- `repository:` — "Source Code" link card on the post page
- `sectionLabel:` — groups the post under a named slice of the archive

```markdown:symbol
examples/BlogSiteFirstPostExample/snippets/stage2.md
```

`redirectUrl:` is also available for migrated posts; see <xref:how-to.pages.redirects>. For the full record definition, see <xref:reference.api.blog-site-front-matter>.

</Step>
<Step StepNumber="2">

**Reload and confirm every surface updated**

With the file saved, reload the running site and verify each new field in turn. No code changes are needed — the host from the scaffold tutorial stays untouched.

</Step>
</Steps>

<Checkpoint>

- Visit `http://localhost:5000/blog/my-first-post/` — the post header shows the byline **Author Name**, the series banner **Pennington Field Notes**, three tag chips (**pennington**, **dotnet**, **blogging**), and a **Source Code** link card pointing at the `repository:` URL.
- Visit `http://localhost:5000/tags/pennington/` — the post appears on the per-tag index; repeat for `/tags/dotnet/` and `/tags/blogging/`.
- Visit `http://localhost:5000/archive` — the archive card carries the longer description from the expanded front matter.
- Visit `http://localhost:5000/rss.xml` — the feed `<channel>` carries the site title and one `<item>` whose `<title>`, `<description>`, `<pubDate>`, `<author>`, and `<link>` all map back to the front matter (`EnableRss` defaults to `true`; for turning the feed off, see <xref:reference.api.blog-site-options>).

</Checkpoint>

---

## Summary

- A Pennington blog post backed by `BlogSiteFrontMatter` maps predictably onto each blog surface — title/description/date for listings, author for byline and RSS, tags for `/tags/<tag>/` indexes, series for the shared banner, repository for the source-code link card.
- `AddBlogSite` binds `AddMarkdownContent<BlogSiteFrontMatter>`, the BlogSite-specific front-matter record. It is parallel to the core `BlogFrontMatter` (not an inheritor) and implements the same capability interfaces with the extra fields BlogSite needs.
- Dropping a new `Content/Blog/*.md` file brings it straight to the home page, the archive, every tag it claims, and `/rss.xml` — no `Program.cs` changes needed.

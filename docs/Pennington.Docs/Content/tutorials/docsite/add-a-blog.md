---
title: "Add a blog to your documentation site"
description: "Drop a Content/blog folder into a DocSite and watch the blog index, post pages, browse-by-tag pages, and RSS feed light up — no Program.cs changes."
sectionLabel: "Getting Started with DocSite"
order: 5
tags:
  - docsite
  - blog
  - authoring
  - rss
uid: tutorials.docsite.add-a-blog
---

By the end of this tutorial the DocSite at `http://localhost:5000/` carries a **Blog** link in its header, a `/blog` index listing two posts newest-first, post pages with a date and byline, browse-by-tag pages, and an RSS feed at `/rss.xml`.

The blog activates from content alone — a folder named `blog` under `Content/`. There is no `Program.cs` change anywhere in this tutorial; every step is a markdown file.

## Prerequisites

- .NET 10 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) — or any DocSite host ready to run

The finished code for this tutorial lives in [`examples/DocSiteBlogExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteBlogExample).

---

## 1. Write your first post

A DocSite turns on its blog when it finds a folder named `blog` under `Content/`. Create that folder, drop one post into it, and the blog appears.

<Steps>
<Step StepNumber="1">

**Create `Content/blog/launching-the-docs.md`**

Add a `blog` folder under `Content/`, alongside your area folders, and create `launching-the-docs.md` inside it. The filename minus `.md` becomes the post's URL slug, so this post serves at `/blog/launching-the-docs`.

> [!NOTE]
> The folder must be named exactly `blog`, in lowercase. On a case-sensitive filesystem `Blog` or `blogs` is not detected. This applies to DocSite's content-detected blog; the standalone BlogSite template instead reads `BlogSiteOptions.BlogContentPath`, which defaults to `Blog`.

</Step>
<Step StepNumber="2">

**Paste in the post**

Paste the markdown below. The four front-matter fields are the ones every post uses: `title` is the post heading and link label, `description` is the summary shown on the blog index, `author` is the byline, and `date` is the publication date.

```markdown:symbol
examples/DocSiteBlogExample/snippets/stage1.md
```

</Step>
<Step StepNumber="3">

**Run the host**

```bash
dotnet run
```

Open `http://localhost:5000/`.

</Step>
</Steps>

<Checkpoint>

- A **Blog** link appears in the site header, beside the theme toggle.
- `http://localhost:5000/blog` lists one post — **Launching the Harbor docs** — with its date and description.
- `http://localhost:5000/blog/launching-the-docs` renders the post body under its title, with the **By Dana Reyes** byline.

</Checkpoint>

---

## 2. Add a second post

The blog index orders posts by `date`, newest first. Add a second, more recent post and watch it take the top slot.

<Steps>
<Step StepNumber="1">

**Create `Content/blog/whats-next.md`**

Add a second file in the `blog` folder with the markdown below. Its `date` — `2026-05-15` — is a week after the first post.

```markdown:symbol
examples/DocSiteBlogExample/snippets/stage2.md
```

</Step>
<Step StepNumber="2">

**Reload the blog index**

A markdown edit needs no restart — the host watches `Content/`. Save the file and reload `http://localhost:5000/blog`.

</Step>
</Steps>

<Checkpoint>

- `http://localhost:5000/blog` now lists two posts.
- **What's next for Harbor** sits above **Launching the Harbor docs** — the newer `date` puts it first.

</Checkpoint>

---

## 3. Tag your posts and find the feed

A `tags` list on a post adds it to browse-by-tag pages. Tag both posts, then look at the feed the blog has been publishing all along.

<Steps>
<Step StepNumber="1">

**Add tags to the first post**

Add a `tags` block to `launching-the-docs.md`. The file now reads:

```markdown:symbol
examples/DocSiteBlogExample/Content/blog/launching-the-docs.md
```

</Step>
<Step StepNumber="2">

**Add tags to the second post**

Give `whats-next.md` its own `tags` block. One tag, `announcements`, is shared with the first post.

```markdown:symbol
examples/DocSiteBlogExample/Content/blog/whats-next.md
```

</Step>
<Step StepNumber="3">

**Reload and follow the tags**

Save both files and reload `http://localhost:5000/blog/launching-the-docs`.

</Step>
</Steps>

<Checkpoint>

- The post page now lists its tags beneath the body — **launching-the-docs** shows **announcements** and **docs** as links.
- Following a tag opens its page — `http://localhost:5000/blog/tags/announcements` lists both posts; `/blog/tags/docs` lists only the first.
- The blog index carries a **Browse by tag** link to `http://localhost:5000/blog/tags`, which lists all three tags with their post counts — **announcements** (2), **docs** (1), and **roadmap** (1).
- `http://localhost:5000/rss.xml` is a valid RSS feed — a `<channel>` with the site title and two `<item>` elements whose `<title>`, `<description>`, `<pubDate>`, and `<author>` come from the front matter. `CanonicalBaseUrl` makes the `<link>` URLs absolute.
- Run `dotnet run -- build` — the static build writes `blog/index.html`, the two post pages, the `blog/tags/` pages, and `rss.xml` into `output/`.

</Checkpoint>

---

## Summary

- A folder named `blog` under `Content/` is the whole switch — `AddDocSite` finds it at startup and turns on the blog index, post pages, tag pages, the RSS feed, and the header link.
- Each `BlogPostFrontMatter` field drives a surface: `title`, `description`, and `date` for the index; `author` for the byline and RSS; `tags` for the `/blog/tags/` pages.
- Posts sort newest-first by `date`.
- `/rss.xml` is generated automatically; `CanonicalBaseUrl` makes its links absolute.
- None of it needed a `Program.cs` change — the blog is pure content.

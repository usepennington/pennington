---
title: "Author your first post with BlogSiteFrontMatter"
description: "Write a Pennington blog post with BlogSiteFrontMatter, see it on the index, and turn on the built-in RSS feed."
section: "blogsite"
order: 20
tags: []
uid: tutorials.blogsite.first-post
isDraft: true
search: false
llms: false
---

> **In this page.** Writing a post with `BlogSiteFrontMatter` (title, description, date, author, tags, series, repository, section, redirectUrl), seeing the post appear on the blog index, and enabling the built-in RSS feed. `AddBlogSite` binds posts through `AddMarkdownContent<BlogSiteFrontMatter>` — not the core `BlogFrontMatter` — so this is the record the tutorial teaches.
>
> **Not in this page.** Customizing the post template or adding a tag-index page.

## What you'll do

- **Artifact** (one sentence): a Pennington blog site with a new dated post authored by you, visible at `/blog/<slug>`, listed on the blog index and `/archive`, and syndicated in `/rss.xml`.
- **Skill** (one sentence): you'll know how to author a `BlogSiteFrontMatter` YAML header for a Markdown post and how to verify the post reaches the blog index, the archive, and the RSS feed.

## Prerequisites

- Bullets to cover under Prerequisites:
- .NET 11 SDK installed
- Completed [Scaffold a blog with BlogSite](/tutorials/blogsite/scaffold) (or have an equivalent BlogSite project with `AddBlogSite` + `UseBlogSite` + `RunBlogSiteAsync` wired up)
- A text editor that can edit Markdown
- The finished code for this tutorial lives in `examples/AlexBlogExample` (reference project in the Pennington repo).

---

## 1. Create the post file

- Bullets to cover under this unit:
- Locate `Content/Blog/` inside the scaffolded project — the directory wired up by `BlogSiteOptions.ContentRootPath = "Content"` + `BlogSiteOptions.BlogContentPath = "Blog"`.
- Explain that `AddBlogSite` calls `AddMarkdownContent<BlogSiteFrontMatter>` internally against that path, with `BasePageUrl = BlogSiteOptions.BlogBaseUrl` (default `/blog`).
- File name becomes the URL slug — e.g. `hello-world.md` routes to `/blog/hello-world`.
- Create a new file `Content/Blog/hello-world.md`.
- Leave the file empty for now; the next step fills in the YAML front matter.

### Step 1.1 — Add the post file

- Single action: create `Content/Blog/hello-world.md` with an empty body.

### Checkpoint — What you should see now

- A new `hello-world.md` file under `Content/Blog/`.
- Running `dotnet run` still succeeds, but visiting `/blog/hello-world` either shows an empty page or a diagnostic — that's expected until front matter is added in step 2.

---

## 2. Fill in the `BlogSiteFrontMatter` header

- Bullets to cover under this unit:
- `BlogSiteFrontMatter` is the record `AddBlogSite` binds YAML front matter into for every post; its members map one-to-one to YAML keys via `CamelCaseNamingConvention`.
- Name this explicitly against the core `BlogFrontMatter`: `BlogFrontMatter` is the generic blog record for direct `AddMarkdownContent<BlogFrontMatter>` use; `BlogSiteFrontMatter` is the template-specific superset `AddBlogSite` wires up and is what this tutorial teaches.
- Show the full list of supported keys and their types:
  - `title` (string, default `"Empty title"`)
  - `author` (string, default `""` — overrides `BlogSiteOptions.AuthorName` per post)
  - `description` (string?, optional — used for SEO and RSS `<description>`)
  - `date` (`DateTime?`, optional — drives ordering and RSS `<pubDate>`)
  - `tags` (string list, optional — routed to the tags index)
  - `series` (string, default `""` — groups posts in a named series)
  - `repository` (string, default `""` — when non-empty, renders a "source repository" link card on the post via `BlogPost.razor`)
  - `section` (string?, optional — participates in `ISectionable` grouping)
  - `redirectUrl` (string?, optional — emits a redirect stub via `IRedirectable`)
  - `uid` (string?, optional — cross-reference target)
  - `isDraft` (bool, default `false` — drafts are skipped during `build`)
  - `search` (bool, default `true`)
  - `llms` (bool, default `true`)
- Emphasize that nothing is strictly required; `Title` defaults to `"Empty title"` if omitted, but every real post sets it.
- Tell the reader to paste the front matter block below into the top of `hello-world.md` and replace the author name with their own.
- Below the `---` closer, add a one-paragraph body in Markdown.

### Step 2.1 — Reference the `BlogSiteFrontMatter` record

- Show the actual record definition the reader's YAML will bind to, so they can see every supported property — including `Repository`, `RedirectUrl`, and `Section` that are specific to the blog template.
- Snippet (raw-file fence): `src/Pennington.BlogSite/BlogSiteFrontMatter.cs` — full record definition.

```csharp raw="src/Pennington.BlogSite/BlogSiteFrontMatter.cs"
```

### Step 2.2 — Write the YAML front matter

- Show a real, complete front matter + body pair pulled from the canonical blog example.
- Snippet (raw-file fence): `examples/AlexBlogExample/Content/Blog/building-a-cli-part-1.md` — demonstrates the core blog keys (title, date, author, description, tags, series) in action plus a rendered body.
- Pair it with a minimal hand-written block demonstrating the template-only keys (`repository`, `section`) so the reader sees how `BlogSiteFrontMatter` extends a plain blog record.

```markdown raw="examples/AlexBlogExample/Content/Blog/building-a-cli-part-1.md"
```

```yaml
---
title: "Hello, world"
date: 2026-04-13
author: "Your Name"
description: "First post using BlogSiteFrontMatter"
tags: ["meta"]
repository: "https://github.com/your-handle/your-repo"
---
```

- Instruct the reader to model their own `hello-world.md` after these — change the `title`, set today's `date` in ISO format (`YYYY-MM-DD`), set `author` to their name, add one or two `tags`, drop `series` for a standalone post, and leave `repository` off unless they want the source-code link card to render.

### Checkpoint — What you should see now

- Run `dotnet run` and visit `http://localhost:5000/blog/hello-world`.
- You should see the post title, date, author, and rendered Markdown body.
- If `repository` is set, a "source repository" link card appears near the footer of the post (rendered by `BlogPost.razor`).
- Visit `http://localhost:5000/blog` and confirm the post appears in the chronological index, sorted by `date` descending.
- Visit `http://localhost:5000/archive` and confirm the post appears in the full archive listing.

---

## 3. Enable and verify the built-in RSS feed

- Bullets to cover under this unit:
- `BlogSiteOptions.EnableRss` is `true` by default, so an RSS feed ships for free — but it relies on `CanonicalBaseUrl` being set to produce absolute URLs.
- `UseBlogSite` maps `/rss.xml` as a minimal API endpoint that serializes every non-draft post via `BlogSiteContentService.GetRssXmlAsync`.
- The feed's `<pubDate>` comes from the post's `date` field — omit it and the post will still render but won't syndicate cleanly.
- `BlogSite` also injects `<link rel="alternate" type="application/rss+xml" href="/rss.xml" />` into every page `<head>`, so feed readers can auto-discover it.

### Step 3.1 — Confirm `EnableRss` and set `CanonicalBaseUrl`

- Show the relevant wiring snippet from `AlexBlogExample/Program.cs` so the reader can confirm both options are set.
- Snippet (raw-file fence): `examples/AlexBlogExample/Program.cs` — shows `EnableRss = true`, `CanonicalBaseUrl`, `BlogBaseUrl`, and the `AddBlogSite`/`UseBlogSite`/`RunBlogSiteAsync` trio.

```csharp raw="examples/AlexBlogExample/Program.cs"
```

- Explain the one non-obvious line: `CanonicalBaseUrl` must be an absolute `https://` URL so the feed's `<link>` elements resolve from any feed reader. Leaving it blank produces a feed that validates but has unusable relative links.

### Step 3.2 — Request the feed

- Tell the reader to run `dotnet run` and open `http://localhost:5000/rss.xml` in a browser or `curl` it.

### Checkpoint — What you should see now

- The browser shows a valid RSS 2.0 XML document with a `<channel>` whose `<title>` matches `BlogSiteOptions.SiteTitle`.
- The post you created in step 2 appears as an `<item>` with the `<title>`, `<description>`, `<pubDate>`, and `<link>` derived from its front matter.
- View source on any blog page and confirm `<link rel="alternate" type="application/rss+xml" href="/rss.xml" />` is present in the `<head>`.

---

## Summary

- You authored a Markdown post with a complete `BlogSiteFrontMatter` YAML header and saw it render at `/blog/<slug>`.
- You verified the post appears on the blog index (sorted by `date` descending) and on `/archive`.
- You confirmed the built-in `/rss.xml` feed includes your post as an `<item>` and that `<link rel="alternate">` feed-discovery tags are injected into every page.
- You know which YAML keys `BlogSiteFrontMatter` accepts — including the template-only `repository`, `redirectUrl`, and `section` — and which are optional.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.

# BlogSiteFirstPostExample

Extends the scaffold by populating a real post end-to-end. Host shape is identical to the scaffold — the teaching surface is `Content/Blog/my-first-post.md`, where every field on `BlogSiteFrontMatter` a reader will touch is filled with a meaningful value.

## Concepts

- `BlogSiteFrontMatter` (title, date, author, tags, series, repository, summary, sectionLabel, redirectUrl)
- `EnableRss` / `EnableSitemap` (defaults true; set explicitly so the tutorial has a symbol to point at)
- `CanonicalBaseUrl` driving absolute URLs in RSS / sitemap

## Field semantics (what's visible vs. data-only)

A reader populating every field on one post sometimes wonders why some fields produce no visible chrome. The reasons aren't bugs:

- **`series:`** — `BlogPost.razor` renders a "This post is part of a series" panel listing every post that shares the series name. With one post in `Pennington Field Notes`, the panel still appears with a single bolded entry (visible at `/blog/my-first-post/`). Add a second post with the same `series:` value to see the list grow.
- **`sectionLabel:`** — groups the post under a named section in the archive listing. With one post, there's no peer to group against, so the rendered effect of `sectionLabel: field-notes` is invisible until a second post sets the same label.
- **`redirectUrl:`** — when **set** to a URL, the page emits a client-side redirect (meta-refresh + `<link rel="canonical">`) to that target instead of rendering the post — useful for migrating an old URL to a new home. The key is left **blank** in `my-first-post.md` on purpose: that's the "this post stays here" state. Set it to `https://example.com/new-home/` to see the redirect chrome.
- **`repository:`** — a string the byline/footer chrome can render as a "View source" link (template-specific). The kitchen-sink BlogSite renders it as a small repo icon link below the post title.

## Staged content

Markdown stages under `snippets/` as `stage1.md` → `stage2.md`.

## Referenced from

- `docs/.../tutorials/blogsite/first-post.md`
- `docs/.../reference/front-matter/keys.md` (blog-only keys)

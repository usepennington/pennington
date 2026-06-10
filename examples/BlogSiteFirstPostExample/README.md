# BlogSiteFirstPostExample

Extends the scaffold by populating a real post end-to-end. Host shape is identical to the scaffold — the teaching surface is `Content/Blog/my-first-post.md`, where each `BlogSiteFrontMatter` field that drives a visible blog surface is filled with a meaningful value.

## Concepts

- `BlogSiteFrontMatter` (title, date, description, author, tags, series, repository)
- `EnableRss` / `EnableSitemap` (defaults true; set explicitly so the tutorial has a symbol to point at)
- `CanonicalBaseUrl` driving absolute URLs in RSS / sitemap

## Field semantics

Each field in `my-first-post.md` drives a surface the tutorial's checkpoints verify:

- **`series:`** — `BlogPost.razor` renders a "This post is part of a series" panel listing every post that shares the series name. With one post in `Pennington Field Notes`, the panel still appears with a single bolded entry (visible at `/blog/my-first-post/`). Add a second post with the same `series:` value to see the list grow.
- **`repository:`** — `BlogPost.razor` renders a "Source Code" link card below the post body pointing at this URL.
- **`tags:`** — populate the `/tags/<tag>/` index pages and the tag chips on the post page.
- **`author:`** — drives the byline and the RSS `<author>` element.

`BlogSiteFrontMatter` also carries `sectionLabel` and `redirectUrl`, but the BlogSite template renders neither (the archive listing is flat by date; `redirectUrl` is honored by the response pipeline, not the post chrome), so this example leaves them out. They are documented in the front-matter keys reference.

## Staged content

Markdown stages under `snippets/` as `stage1.md` → `stage2.md`.

## Referenced from

- `docs/.../tutorials/blogsite/first-post.md`
- `docs/.../reference/front-matter/keys.md` (blog-only keys)

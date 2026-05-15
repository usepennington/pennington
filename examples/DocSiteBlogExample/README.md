# DocSiteBlogExample

A DocSite with a root `Content/index.md` landing page, a single `Guides` area, and a `Content/blog/` folder. Dropping markdown posts into `blog/` activates the DocSite blog — the index at `/blog`, post pages, browse-by-tag pages under `/blog/tags/`, the `/rss.xml` feed, and the "Blog" header link — with no wiring in `Program.cs`.

## Concepts

- `Content/index.md` serving the site root at `/`.
- The `Content/blog/` folder convention — `AddDocSite` detects it at startup and activates the blog.
- `BlogPostFrontMatter` — `title`, `description`, `author`, `date`, `tags`.
- Date-descending ordering on the blog index.
- `tags:` driving `/blog/tags` and per-tag listing pages.
- `CanonicalBaseUrl` driving absolute post URLs in `/rss.xml`.

## Staged content

Markdown stages under `snippets/` as `stage1.md` (first post, before tags) and `stage2.md` (second post, before tags). The final tagged posts live in `Content/blog/`.

## Referenced from

- `docs/.../tutorials/docsite/add-a-blog.md`

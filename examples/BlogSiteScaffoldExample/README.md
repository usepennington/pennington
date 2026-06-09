# BlogSiteScaffoldExample

The smallest possible BlogSite. `AddBlogSite` / `UseBlogSite` / `RunBlogSiteAsync` give you a home page, `/archive`, `/blog/<slug>`, `/tags`, `/tags/<name>`, and `/rss.xml` from one set of `BlogSiteOptions` and one markdown file under `Content/Blog/`.

## Concepts

- BlogSite template **defaults** — `BlogContentPath = "Blog"`, `BlogBaseUrl = "/blog"`, `TagsPageUrl = "/tags"`. The scaffold's `Program.cs` does **not** assign these (that's the point of a scaffold); see `reference/blogsite/options.md` for the full surface.
- Author identity (`AuthorName`, `AuthorBio`) feeding RSS and JSON-LD
- `UseBlogSite` ordering — antiforgery, static files, Razor routing, MonorailCSS, Pennington middleware
- One `tags:` entry on `hello-world.md` keeps the `/tags/<name>` route discoverable from the post page (the post body links to `/tags/scaffold/` via `BlogPost.razor`'s tag chip) — drop the front-matter line and that link disappears.
- A root `Content/404.md` (outside `Content/Blog/`) — the not-found body. BlogSite renders it from its root catch-all for any unmatched URL and writes it to `output/404.html` during the static build.

## Tutorial stages

`Stage1_BeforeAddBlogSite.cs` → `Stage2_AddBlogSiteOnly.cs` → `Stage3_UseBlogSite.cs`.

## Referenced from

- `docs/.../tutorials/blogsite/scaffold.md`
- `docs/.../reference/blogsite/routes.md`

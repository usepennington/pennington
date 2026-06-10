---
title: "Provide a 404 page"
description: "Author the not-found body with a content-root 404.md (or a NotFound.razor component); the static build writes it to output/404.html for your host to serve."
uid: how-to.pages.not-found
order: 6
sectionLabel: "Pages"
tags: [not-found, 404, deployment, static-build]
---

A static host serves a single `404.html` for any URL it can't find. You supply that page's body with one file — no server-side code, no error-handling route. Pennington's build renders it and writes `output/404.html` for you.

## Add a `404.md`

Drop a `404.md` at your content root. Give it a title and a short body, and point readers somewhere useful.

```markdown:symbol
examples/DocSiteScaffoldExample/Content/404.md
```

Run `dotnet run -- build` and the file lands at `output/404.html`. The file is reserved: there is no `/404/` route, and it never appears in navigation, the sitemap, search, or `llms.txt`. BlogSite works the same way — put `404.md` at the content root (outside the blog folder) and it becomes the site's not-found body.

> [!TIP]
> Don't gold-plate the 404. On a static host a reader reaches it only by following a dead link or mistyping a URL, and your host serves the same `404.html` for every miss. A title, a sentence, and a link home is plenty. You'll see it often in development; your readers almost never will.

## Why there's no `/404/` route

A routable `/404/` would be a valid route whose job is to announce an invalid destination, and nothing runs on a static host to choose it; instead the body renders at the catch-all and reaches readers only through your host's `404.html` mapping. For how the build materializes that file, see <xref:explanation.core.dev-vs-build>.

## Use a Razor component instead

When you want components or richer markup, add a `NotFound.razor` (no `@page` directive). The catch-all finds it by name and renders it for any unmatched URL.

```razor:symbol
examples/DocSiteChromeOverridesExample/Components/NotFound.razor
```

If both a `404.md` and a `NotFound.razor` exist, the markdown file wins. With neither, Pennington renders a built-in localized message, so every site still produces a valid `404.html`.

## Make your host serve it

Producing `404.html` is half the job — your host has to return it for unknown URLs. The mapping differs per host:

- [GitHub Pages](xref:how-to.deployment.github-pages) serves a root `404.html` automatically.
- [Other managed hosts](xref:how-to.deployment.adapt-for-other-hosts) (Netlify, Cloudflare Pages, Azure Static Web Apps) need a fallback rule in their config.
- [Nginx or IIS](xref:how-to.deployment.self-host) need an `error_page` / fallback directive.

## Verify

- Run `dotnet run -- build` and confirm `output/404.html` contains your content.
- Run `dotnet run`, visit a URL that doesn't exist, and confirm you see the body with an HTTP 404 status (`curl -I http://localhost:5000/nope`).

## Related

- How-to: [Build a static site](xref:how-to.deployment.static-build) — where `output/404.html` comes from in the build.
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build) — how the crawler materializes `404.html`.
- Background: [The response pipeline](xref:explanation.core.response-processing) — how the rendered 404 body keeps an HTTP 404 status.

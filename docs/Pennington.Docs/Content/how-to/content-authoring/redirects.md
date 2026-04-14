---
title: "Configure redirects"
description: "Set `redirectUrl` in front matter to emit a meta-refresh stub page with `noindex`."
section: "content-authoring"
order: 120
tags: []
uid: how-to.content-authoring.redirects
isDraft: true
search: false
llms: false
---

> **In this page.** Setting `redirectUrl:` in `DocSiteFrontMatter` to emit a meta-refresh stub page with `noindex`.
>
> **Not in this page.** HTTP 301 responses at the hosting layer or batch redirects from a sidecar file.

## When to use this

When you have renamed or moved a page and want incoming links to the old URL to land on the new one. The redirect works on both `dotnet run` (dev serve) and `dotnet run -- build` (static output).

## Assumptions

- You have an existing Pennington doc site wired via `AddDocSite` + `UseDocSite` + `RunDocSiteAsync`.
- Your front-matter type implements `IRedirectable` — `DocSiteFrontMatter` and `BlogSiteFrontMatter` already do.
- You know the canonical target URL the old page should redirect to (use site-relative paths such as `/getting-started/`).

To copy a working setup, see `examples/BeaconDocsExample` — it ships a redirect at `Content/setup.md` pointing to `/getting-started/`.

---

## Steps

### 1. Identify the old URL you want to keep alive

Keep the source markdown file at its existing path so its URL (the redirect source) is preserved. Do not delete the file — the redirect is emitted from that file's front matter. The target URL in `redirectUrl:` should be site-relative (leading `/`) and typically end with a trailing slash.

### 2. Replace the page body with redirect front matter

Strip the file down to YAML front matter; the body is ignored once `redirectUrl` is set (a stub page is emitted instead). The only required key beyond `title:` is `redirectUrl:`. Leave a one-line fallback link in the body for users whose browsers ignore meta refresh.

```yaml
---
title: "Setup (Moved)"
description: "This page has moved"
redirectUrl: "/getting-started/"
---

This page has moved to [Getting Started](/getting-started/).
```

For a purely internal redirect where no human will see the stub, a bare `redirectUrl:` with no title is enough:

```yaml
---
redirectUrl: /sub-folder/page-one
---
```

### 3. Implement `IRedirectable` if you're using a custom record

If your site uses a custom front-matter record instead of `DocSiteFrontMatter`, add `IRedirectable` to the interface list and expose `string? RedirectUrl`. `DocSiteFrontMatter` and `BlogSiteFrontMatter` already implement it — only custom records need this step.

```csharp:xmldocid
T:MultipleContentSourceExample.DocsFrontMatter
```

### 4. Know what the engine emits

The output is an HTML stub at the old URL with `<meta http-equiv="refresh" content="0;url={target}">` and `<link rel="canonical" href="{target}">`. The same source file is automatically excluded from the TOC, sidebar, search index, `sitemap.xml`, and `llms.txt`.

---

## Verify

- Run `dotnet run --project <your-site>` and visit the old URL; confirm the browser follows the meta refresh to the target URL.
- Run `dotnet run --project <your-site> -- build` and open `output/<old-path>/index.html`; confirm it contains `<meta http-equiv="refresh" content="0;url=/<target>/">`.
- Open `output/sitemap.xml` and confirm the redirect URL is absent.
- Open `output/search-index-<locale>.json` and confirm the redirect URL is absent.

## Related

- Reference: [Front matter keys](/reference/front-matter/keys) — the `redirectUrl` key.
- Background: [Front-matter capabilities](/explanation/core/front-matter-capabilities)

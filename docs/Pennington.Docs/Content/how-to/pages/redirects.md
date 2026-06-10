---
title: "Forward visitors from a renamed page"
description: "Set redirectUrl in front matter to forward visitors from the old path to the new one — an HTTP 301 on the live server, plus a meta-refresh stub in the static build."
uid: how-to.pages.redirects
order: 4
sectionLabel: "Pages"
tags: [redirects, front-matter, routing]
---

When a published page is renamed or deleted, set `redirectUrl:` in its front matter to forward visitors to the new URL. On the dev and self-hosted server, Pennington issues a real HTTP 301 from the old path; the page body is not rendered or indexed. The static build also writes a `<meta http-equiv="refresh">` stub at the old path, since a static host can't issue a server-side 301 without its own redirect config.

When the page is *deleted* rather than renamed, keep the file as a front-matter-only stub at the old path — `title:` and `redirectUrl:` with no body — so the old URL still resolves to a redirect. Delete the file and the old URL 404s.

`redirectUrl:` accepts an external absolute URL (`https://…`) as well as an internal path; the value is emitted verbatim as the 301 `Location` and the meta-refresh target, so a cross-site redirect works the same way.

For batch redirects across many paths, configure them at the hosting layer instead — Nginx or IIS rules (see <xref:how-to.deployment.self-host>) or a Netlify/Cloudflare/Azure rules file (see <xref:how-to.deployment.adapt-for-other-hosts>).

## Before you begin
- An existing Pennington site using `AddDocSite` (see <xref:tutorials.docsite.scaffold> if not) or another host whose front-matter type implements `IRedirectable`. `DocSiteFrontMatter` and `BlogSiteFrontMatter` both do. For a custom record, add the interface — see <xref:explanation.core.front-matter-capabilities>.
- Both the old URL (the page being retired) and the new URL (the canonical destination) are known.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample), whose `Content/main/redirect-source.md` is a complete redirect page.

## Add `redirectUrl:` to the old page

Open the markdown file at the old URL and set `redirectUrl:` to the new absolute path. Keep `title:` so diagnostics stay readable; the body does not render.

```markdown:symbol
examples/DocSiteKitchenSinkExample/Content/main/redirect-source.md
```

## Verify

- Run `dotnet run` and visit the old URL: the page redirects immediately to the target set in `redirectUrl`.
- View source on the old URL: the markup contains `<meta http-equiv="refresh" content="0;url=...">` and a `<link rel="canonical" href="...">` pointing at the redirect target.
- Check `/sitemap.xml` and `/llms.txt`: the old URL does not appear. A redirect has no canonical HTML page, so it is excluded from both.

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys) — the row for `redirectUrl` (type, default, which records support it).
- Reference: [`IFrontMatter` and capability defaults](xref:reference.api.i-front-matter) — how `IRedirectable` fits alongside the other capability interfaces.
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities) — why `IRedirectable` stayed a separate capability instead of collapsing into `IFrontMatter`.

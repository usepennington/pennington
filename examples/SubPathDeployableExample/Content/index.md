---
title: Welcome
description: The deployment demo landing page — verify that internal anchors, assets, and scripts rewrite when the site is built with a sub-path base URL.
order: 10
---

This is the landing page for **SubPathDeployableExample**, the demo app that
backs every how-to under `/how-to/deployment/*`.

The teaching artefacts for those pages are **sibling fixture files** of this
project — you can copy them verbatim into your own repo:

- `.github/workflows/deploy.yml` — GitHub Pages Actions workflow
- `staticwebapp.config.json` — Azure Static Web Apps
- `netlify.toml` — Netlify
- `nginx.conf` — self-host behind Nginx
- `web.config` — self-host behind IIS

## Verify the base URL

Jump to the [first guide page](/guides/first-page/) and inspect the generated
HTML. When the site is built with a sub-path — for example
`dotnet run -- build /my-sub-path` — every root-relative anchor, stylesheet,
and script on this page is prefixed with `/my-sub-path` by `BaseUrlHtmlRewriter`.

When the site is built at the root (`dotnet run -- build`), the same links
stay unprefixed. One HTTP pipeline, two outputs.

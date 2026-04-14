---
title: "Adapt the deploy workflow for other hosts"
description: "Reuse the GitHub Pages recipe on Azure Static Web Apps, Cloudflare Pages, and Netlify — shared shape plus a per-host delta table."
section: "deployment"
order: 30
tags: []
uid: how-to.deployment.adapt-for-other-hosts
isDraft: true
search: false
llms: false
---

> **In this page.** How to port the [GitHub Pages recipe](/how-to/deployment/github-pages) to Azure Static Web Apps, Cloudflare Pages, and Netlify — the shared shape, a per-host delta table, and one short snippet per host.
>
> **Not in this page.** Full walkthroughs of each provider's dashboard, Azure Functions API backends, Cloudflare Workers in front of the static output, or Netlify Functions — Pennington emits a static site and stops there.

## When to use this

You already have the GitHub Pages recipe working (or understand its shape) and need the same build pipeline on Azure Static Web Apps, Cloudflare Pages, or Netlify. Read [Deploy to GitHub Pages](/how-to/deployment/github-pages) first — this page only calls out the per-host deltas.

## Assumptions

- You have a working Pennington site and `dotnet run --project <site> -- build <baseUrl> <output>` produces `output/` locally.
- You have read [Deploy to GitHub Pages](/how-to/deployment/github-pages) for the shared shape.
- You have admin access to whichever host you are targeting and your repository is reachable by it (pushed to GitHub/GitLab, or Direct Upload).

---

## The shared shape

Every host runs the same command — `dotnet run --project <site> -- build <baseUrl> <output>` — and uploads the emitted directory. What changes per host: how you resolve .NET 11, where you declare the build settings, and what sidecar config file (if any) shapes routing.

## Per-host deltas

| Host | Build command surface | Publish directory field | .NET SDK pin | Extra config file |
|---|---|---|---|---|
| Azure Static Web Apps | GitHub Actions workflow using `Azure/static-web-apps-deploy@v1` + `skip_app_build: true` | `output_location` | `actions/setup-dotnet@v4` preceding the deploy step | `staticwebapp.config.json` for routes, 404, MIME types |
| Cloudflare Pages | Pages dashboard "Build command" field | "Build output directory" field | `UNSTABLE_PRE_BUILD` env or the V2 build image + `global.json` | — |
| Netlify | `[build] command` in `netlify.toml` | `[build] publish` in `netlify.toml` | `DOTNET_VERSION` env var + `global.json` | `netlify.toml` for redirects / 404 |

---

## Per-host specifics

### Azure Static Web Apps

SWA's Oryx builder does not understand the Pennington CLI, so run the build in a preceding step and set `skip_app_build: true` on the deploy action. Without that flag, SWA tries to rebuild and ships an empty site. Run `dotnet run -- build / <output_location>` before `Azure/static-web-apps-deploy@v1`, and drop `staticwebapp.config.json` inside `output_location` so it lands at the deployed root.

```yaml
- name: Build Pennington site
  run: dotnet run --project src/MySite -- build / src/MySite/wwwroot

- name: Deploy
  uses: Azure/static-web-apps-deploy@v1
  with:
    azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
    repo_token: ${{ secrets.GITHUB_TOKEN }}
    action: upload
    app_location: "src/MySite"
    output_location: "wwwroot"
    skip_app_build: true
```

```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/_spa-data/*", "/*.{css,js,json,xml,txt,png,jpg,jpeg,svg,webp,woff,woff2,ico}"]
  },
  "responseOverrides": {
    "404": {
      "rewrite": "/404.html",
      "statusCode": 404
    }
  }
}
```

### Cloudflare Pages

In the Pages dashboard, set framework preset to **None** (presets like Hugo/Jekyll inject wrong defaults), production branch to whichever branch you publish from, and fill the build settings with the exact command you ran locally. Cloudflare's default build image does not ship .NET 11, so pin the SDK via the `UNSTABLE_PRE_BUILD` env var (or switch to the V2 build image and commit a `global.json`).

```text
Build command:
  dotnet run --project src/MySite --configuration Release -- build / output

Build output directory:
  src/MySite/output
```

```text
Environment variable name   Value
UNSTABLE_PRE_BUILD          curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 11.0 --install-dir $HOME/.dotnet && export PATH="$HOME/.dotnet:$PATH"
```

### Netlify

Put everything in `netlify.toml` at the repo root. Pin the SDK with `DOTNET_VERSION` (matching the major band in a committed `global.json`), set `[build] command` to the same `dotnet run -- build` invocation, and set `[build] publish` to the directory you passed as the third positional argument. Netlify reads `netlify.toml` before any dashboard settings, so you can leave the UI fields blank.

```toml
[build]
  command = "dotnet run --project src/MySite --configuration Release -- build / src/MySite/output"
  publish = "src/MySite/output"

[build.environment]
  DOTNET_VERSION = "11.0"
  DOTNET_NOLOGO = "1"
  DOTNET_CLI_TELEMETRY_OPTOUT = "1"

[[redirects]]
  from = "/*"
  to = "/404.html"
  status = 404
```

---

## Verify

- The provider's build log shows the `BuildReport` line (for example `Build Complete — 42 pages in 3.7s`) and zero failed pages.
- The site loads at the provider's URL with internal links and assets intact.
- Hitting an unknown path returns the generated `404.html` (SWA via `responseOverrides`, Cloudflare via built-in 404 handling, Netlify via the `[[redirects]]` rule).

## Related

- How-to: [Deploy to GitHub Pages](/how-to/deployment/github-pages)
- How-to: [Build a static site](/how-to/deployment/static-build)
- How-to: [Host under a sub-path (base URL)](/how-to/deployment/base-url)
- Reference: [Auxiliary options (`OutputOptions`)](/reference/options/auxiliary-options)

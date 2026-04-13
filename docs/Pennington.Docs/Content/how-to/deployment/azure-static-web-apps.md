---
title: Deploy to Azure Static Web Apps
description: Configuring the SWA pipeline, `app_location`/`output_location`, and handling routes and redirects with `staticwebapp.config.json`.
section: deployment
order: 30
uid: how-to.deployment.azure-static-web-apps
isDraft: true
search: false
llms: false
tags: []
---

> **In this page.** Configuring the SWA pipeline, `app_location`/`output_location`, and handling routes and redirects with `staticwebapp.config.json`.
>
> **Not in this page.** Azure Functions API backends.

## When to use this

- You have a built Pennington site and want to host the static output on Azure Static Web Apps
- You want the GitHub Actions SWA deploy workflow to run `dotnet run -- build` and upload the `output/` tree

## Assumptions

- You have an existing Pennington site that produces a static build via `dotnet run --project <SitePath> -- build <baseUrl> <outputDirectory>`
- Your repo is hosted on GitHub and has a Static Web Apps resource with an `AZURE_STATIC_WEB_APPS_API_TOKEN` secret
- The target SWA plan does not need an API backend (skip `api_location`)

---

## Steps

### 1. Confirm the build command shape

- Pennington exposes a single `build` verb via `RunOrBuildAsync` — positional args are `build [baseUrl] [outputDirectory]`
- Default `baseUrl` is `/`, default `outputDirectory` is `output`
- Non-`build` invocations no-op the build path (dev serve still works locally)
- The SWA workflow will invoke `dotnet run -- build / wwwroot` (or similar) against your site project

### 2. Add the GitHub Actions workflow

- Place the file at `.github/workflows/azure-static-web-apps.yml`
- Use the `Azure/static-web-apps-deploy@v1` action
- Set `app_location` to the repo-relative path containing the already-built static tree (the folder referenced by `output_location` resolves relative to this)
- Set `output_location` to the directory Pennington writes (e.g. `wwwroot` or `output`)
- Leave `api_location` empty — API backends are out of scope for this page
- Add a preceding step that runs `dotnet run --project <SitePath> -- build / <output_location>` before the deploy action

```yaml
name: Azure Static Web Apps CI/CD

on:
  push:
    branches: [main]
  pull_request:
    types: [opened, synchronize, reopened, closed]
    branches: [main]

jobs:
  build_and_deploy:
    if: github.event_name == 'push' || (github.event_name == 'pull_request' && github.event.action != 'closed')
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          submodules: true

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '11.0.x'

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

### 3. Tell SWA not to re-build

- SWA's Oryx builder does not understand the Pennington CLI — rebuilding in-action will fail or ship an empty site
- Set `skip_app_build: true` so SWA only uploads the tree you produced in the previous step
- Keep the `dotnet run -- build` step ordered before the deploy step

### 4. Add `staticwebapp.config.json` for routes and redirects

- Place the file inside `output_location` (so it lands at the root of the deployed site)
- Configure `navigationFallback` so client-side SPA navigation (`Pennington.Islands`) falls back to `/index.html`
- Use `responseOverrides` to map `404` to Pennington's emitted `404.html`
- Use `routes` for redirects and trailing-slash canonicalization
- Use `mimeTypes` for any extensions SWA does not know (e.g. `.webmanifest`)

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
  },
  "routes": [
    {
      "route": "/old-page",
      "redirect": "/new-page",
      "statusCode": 301
    }
  ],
  "mimeTypes": {
    ".webmanifest": "application/manifest+json"
  }
}
```

### 5. Match `baseUrl` to the SWA host

- If deploying to the default `*.azurestaticapps.net` host, pass `/` as `baseUrl`
- If deploying behind a sub-path via a custom domain or reverse proxy, pass that prefix (e.g. `build /docs wwwroot`) so `BaseUrlHtmlRewriter` prefixes internal links correctly
- Rebuild whenever the host path changes — the prefix is baked into emitted HTML at build time

### 6. Commit and push

- Commit `.github/workflows/azure-static-web-apps.yml` and `staticwebapp.config.json`
- Push to the tracked branch; SWA triggers the workflow and deploys the uploaded `output_location`

---

## Verify

- Watch the workflow run in GitHub Actions and confirm the `Build Pennington site` step emits files under `output_location`
- Visit the SWA URL and confirm pages render, assets load, and an unknown path serves the Pennington-emitted `404.html`
- Hit a path defined in `routes` and confirm the 301 redirect fires

## Related

- Reference: [OutputOptions and the `build` CLI](/reference/generation/output-options)
- Background: [Why dev-serve and build share one code path](/explanation/architecture/unified-build-path)

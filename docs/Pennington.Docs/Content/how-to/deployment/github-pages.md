---
title: Deploy to GitHub Pages
description: A ready-to-copy Actions workflow with setup-dotnet@v4, upload-pages-artifact@v3, deploy-pages@v4, and the .nojekyll marker.
section: deployment
order: 20
tags: []
uid: how-to.deployment.github-pages
isDraft: true
search: false
llms: false
---

> **In this page.** A ready-to-copy GitHub Actions workflow that runs the Pennington static build and publishes the output to GitHub Pages, including the `.nojekyll` marker.
>
> **Not in this page.** Custom-domain DNS setup beyond ticking the GitHub Pages checkbox — point your DNS at GitHub's documented Pages IPs yourself.

## When to use this

- You have a Pennington site that already builds locally via `dotnet run -- build` and you want it hosted on GitHub Pages under the repository's `github.io` URL or a project sub-path.
- You want a canonical workflow to drop into `.github/workflows/` without researching which action versions are current.

## Assumptions

- You have a working Pennington site (see [_Create your first site_](/tutorials/getting-started/first-site) if not).
- You already run a successful local build with `dotnet run -- build [baseUrl] [outputDirectory]` (see [_Build a static site_](/how-to/deployment/static-build)).
- The repository is hosted on GitHub and you have admin access to enable Pages.
- You know whether the site ships at the repo root (`https://user.github.io/repo/`) or at a user/organisation root (`https://user.github.io/`) — the first needs a base URL, the second does not.

To copy a working setup, see [`examples/MinimalExample`](https://github.com/Pennington/Pennington/tree/main/examples/MinimalExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Enable GitHub Pages with the Actions source

- Open the repository on GitHub and go to **Settings → Pages**.
- Under **Build and deployment → Source**, pick **GitHub Actions** (not "Deploy from a branch").
- Leave **Custom domain** blank unless you already own one; this page stops at the checkbox.
- Confirm the repository has Pages permissions enabled for workflows (default for public repos).

### 2. Decide the base URL your workflow will pass

- Project site at `https://user.github.io/repo/` — pass `/repo/` as `[baseUrl]` so `BaseUrlHtmlRewriter` prefixes every internal link and asset.
- User or organisation site at `https://user.github.io/` — pass `/` (the default).
- The value must match the path segment Pages serves from; getting this wrong produces broken asset links and a blank page.

### 3. Add the workflow file

Create `.github/workflows/deploy.yml` with the content below. Replace `YourSite.csproj` with the actual project path and `/repo/` with the base URL you chose in step 2 (or `/` for a user site).

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: pages
  cancel-in-progress: true

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '11.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build static site
        run: dotnet run --project src/YourSite/YourSite.csproj --configuration Release -- build /repo/ output

      - name: Add .nojekyll marker
        run: touch src/YourSite/output/.nojekyll

      - uses: actions/upload-pages-artifact@v3
        with:
          path: src/YourSite/output

  deploy:
    needs: build
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - id: deployment
        uses: actions/deploy-pages@v4
```

### 4. Add the `.nojekyll` marker in the workflow

- Pennington does not emit `.nojekyll` itself — the `touch .../output/.nojekyll` step above is required.
- Without it, GitHub Pages runs Jekyll over the output and drops any path segment beginning with `_` (for example `_spa-data/`, `_llms/`), which silently breaks SPA navigation and the llms.txt sidecar files.
- Keep the `touch` step between the build and the upload so the marker lives inside the uploaded artifact.

### 5. Match the output path in every step

- The build writes to `<project>/output` by default (from `OutputOptions.FromArgs` — `args[2]` is the output directory).
- The `touch`, `upload-pages-artifact`, and any later workflow steps must all point at the same directory.
- If you pass a third argument to `-- build`, update every path in the workflow to match.

### 6. Commit, push, and trigger the deploy

- Commit the workflow to `main`; the push triggers the first run.
- Watch **Actions → Deploy to GitHub Pages**; the `deploy` job prints the published URL in its environment badge.
- Re-runs are idempotent — `concurrency.cancel-in-progress` drops older in-flight deploys when you push again.

---

## Verify

- The `deploy` job completes green and the environment card links to `https://<user>.github.io/<repo>/`.
- Loading that URL renders the site's home page with styles and internal links intact (no 404 on `/styles.css` — a giveaway that `[baseUrl]` is wrong).
- Fetching `https://<user>.github.io/<repo>/_spa-data/index.json` returns JSON rather than a 404 — confirms `.nojekyll` was uploaded.

## Related

- Reference: [CLI and build arguments](/reference/host/cli)
- How-to: [Host under a sub-path (base URL)](/how-to/deployment/base-url)
- Background: [Unified dev and build path](/explanation/unified-build-path)

---
title: "Ship it — build and deploy to GitHub Pages"
description: "Run the static build via RunOrBuildAsync with 'build [baseUrl] [output]', inspect the BuildReport, write a GitHub Actions workflow, and push .nojekyll-safe output."
section: "getting-started"
order: 40
tags: []
uid: tutorials.getting-started.deploy-github-pages
isDraft: true
search: false
llms: false
---

> **In this page.** Running the static build via `RunOrBuildAsync(args)` with `build [baseUrl] [output]`, inspecting the `BuildReport`, writing a GitHub Actions workflow, and pushing output that GitHub Pages will serve cleanly.
>
> **Not in this page.** Subdirectory hosting gotchas, other hosts (Netlify, Azure, Docker), or custom base-URL rewriting — each is its own how-to.

## What you'll do

- **Artifact:** a built static copy of your Pennington site under `output/`, plus a `deploy.yml` GitHub Actions workflow that publishes it to GitHub Pages on push to `main`.
- **Skill:** you'll know how Pennington's build command works, what the `BuildReport` tells you, and which steps a GitHub Pages workflow needs.

## Prerequisites

- .NET 11 SDK installed
- Completed [Create your first Pennington site](/tutorials/getting-started/first-site) and [Add your first markdown page](/tutorials/getting-started/first-page)
- A GitHub repository with Pages enabled (Settings → Pages → Source: GitHub Actions)

The finished code for this tutorial lives in [`examples/MinimalExample`](https://github.com/scottsauber/Penn/tree/main/examples/MinimalExample) — the smallest Pennington site that can be built and shipped.

> **Grounding note.** No example project currently ships a `.github/workflows/deploy.yml` nor exposes a named method around `RunOrBuildAsync` (all examples keep it in top-level `Program.cs` statements). Pennington core also does not emit `.nojekyll` automatically today (see `docs/_research/blockers.md`). The steps below therefore use raw-file fences against `examples/MinimalExample/Program.cs` for the `RunOrBuildAsync` call and inline YAML for the workflow, and they walk the reader through creating the `.nojekyll` file explicitly.

---

## 1. Understand the two modes of `RunOrBuildAsync`

- Bullets to cover under this unit:
- `RunOrBuildAsync(args)` inspects `args[0]`. If it's `"build"`, it runs the dev host briefly, resolves `OutputGenerationService`, crawls the live routes, writes HTML/CSS/assets to the output directory, then shuts the host down.
- Any other argument shape falls through to `app.RunAsync()` — the normal dev-server mode.
- `build` takes two optional positional arguments: `[baseUrl]` (e.g., `/my-repo/` for a project-sub-path site) and `[output]` (e.g., `output`). Defaults: base URL `/`, output directory `output`.
- The exit code is non-zero if the `BuildReport` contains errors (broken links, failed pages).

### Step 1.1 — Find the `RunOrBuildAsync` call

- Open `Program.cs` and confirm `await app.RunOrBuildAsync(args);` is the final statement.

```csharp file="examples/MinimalExample/Program.cs"
```

- _This file is the minimal Pennington host: `AddPennington` + `UsePennington` + `RunOrBuildAsync`._

### Checkpoint — you know what "build" means

- You can explain in one sentence what `args[0] == "build"` does.
- You know the order of the two positional arguments.

---

## 2. Run the static build locally

- Bullets to cover under this unit:
- Pick the `baseUrl` you will use on GitHub Pages. For a project site at `https://user.github.io/my-repo/`, that is `/my-repo/`. For a user/organization site, it is `/`.
- Decide an output directory. `output` is conventional.

### Step 2.1 — Run with the build arguments

- From the site project folder, run `dotnet run -- build /my-repo/ output` (substitute your values).
- The host starts briefly, the crawler runs, and the process exits.
- A new `output/` directory contains `index.html`, nested folders per route, and asset files.

### Step 2.2 — Read the `BuildReport` printed to stdout

- The last lines of the command output are the `BuildReport` summary: total pages generated, failed pages, broken links, and the elapsed `Duration`.
- If `FailedPages` > 0 or the broken-link list is non-empty, the exit code is `1` — the workflow later will fail in that case (by design).
- Re-run locally until the summary is clean.

### Checkpoint — you have a clean local build

- `output/index.html` exists.
- `BuildReport` shows `0` failed pages and `0` broken links.
- Opening `output/index.html` in a browser looks right (accounting for the base-URL prefix — links may appear broken when viewed from `file://`).

---

## 3. Add a GitHub Actions workflow

- Bullets to cover under this unit:
- GitHub Pages needs three action steps: `actions/checkout`, `actions/setup-dotnet`, and the pages upload/deploy pair.
- The job uploads a Pages "artifact" from the `output/` directory, then a separate `deploy` job consumes it.

### Step 3.1 — Create `.github/workflows/deploy.yml`

- Add this workflow at the repo root.

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
          dotnet-version: '11.x'
      - name: Build the site
        working-directory: ./your-site
        run: dotnet run -- build /${{ github.event.repository.name }}/ output
      - name: Add .nojekyll
        run: touch ./your-site/output/.nojekyll
      - uses: actions/upload-pages-artifact@v3
        with:
          path: ./your-site/output

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

### Step 3.2 — Add the `.nojekyll` file explicitly

- GitHub Pages runs a Jekyll pass on uploaded content by default, which strips files starting with an underscore (for example, assets under `_framework/`).
- The workflow's `touch ./your-site/output/.nojekyll` step creates an empty marker that disables that pass.
- If you deploy outside GitHub Actions, create the file manually once and commit it to the output pipeline.

### Checkpoint — the workflow is complete

- `.github/workflows/deploy.yml` is committed.
- The permissions block includes `pages: write` and `id-token: write`.
- The `upload-pages-artifact` step points at the directory that matches your `build [output]` argument.

---

## 4. Push, watch the action, and open the Pages URL

- Bullets to cover under this unit:
- Push to `main`. The Action runs: checkout → setup-dotnet → build → upload → deploy.
- The deploy step's `environment.url` becomes the Pages URL once the deploy completes.
- Subsequent pushes re-run the pipeline automatically.

### Step 4.1 — Push and watch the run

- `git push` and open the Actions tab in the repo.
- Expand the `build` job and watch the `BuildReport` summary print.
- Wait for the `deploy` job to finish.

### Step 4.2 — Visit the URL

- Open `https://<user>.github.io/<repo>/` (for a project site).
- The site should render with the same structure you saw locally.

### Checkpoint — the site is live

- Both workflow jobs are green.
- The Pages URL in the deploy step shows your site.
- Internal links work.

---

## Summary

- You ran a local static build, read the `BuildReport`, and produced a clean `output/` directory.
- You wrote a GitHub Actions workflow that builds the site, adds `.nojekyll`, and deploys via the official Pages actions.
- You know how the two build arguments (`[baseUrl]`, `[output]`) map to the GitHub Pages URL shape.
- You know why `.nojekyll` matters and how to add it.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.

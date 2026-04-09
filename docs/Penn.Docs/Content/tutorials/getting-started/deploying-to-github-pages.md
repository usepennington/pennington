---
title: "Deploying to GitHub Pages"
description: "Generate a static site and deploy it to GitHub Pages with a GitHub Actions workflow"
uid: "penn.tutorials.deploying-to-github-pages"
order: 40
---

## Beat 1: Build the site locally

The reader runs the build command with no arguments to generate static output and inspects the result. The goal is to verify the site works as static files before deploying.

### What to show
- Terminal command: `dotnet run -- build`
- Show the expected console output:
  ```
  Build Complete -- 7 pages in 1.2s
    7 pages generated
  ```
- Open `output/index.html` in a browser to verify it renders correctly

### Key points
- The `--` before `build` is necessary to pass the argument to the application rather than to `dotnet run`
- With no base URL argument, all URLs are root-relative (starting with `/`) -- this is correct for root-domain deployment
- If the build reports errors, the process exits with code 1 — useful for failing CI builds.

## Beat 2: Understand the output directory structure

The reader examines the `output/` directory to understand what static generation produces. The goal is to build a mental model of the output structure before configuring deployment.

### What to show
- A directory listing of `output/`:
  ```
  output/
    index.html
    development/
      coding-standards/
        index.html
      pr-process/
        index.html
    operations/
      deployment-checklist/
        index.html
      incident-response/
        index.html
    search-index.json
    sitemap.xml
    404.html
    styles.css
    _content/
      Penn.UI/
  ```
- Each content page becomes a directory with `index.html`. Auxiliary files (`search-index.json`, `sitemap.xml`, `styles.css`, `404.html`) and static assets from Razor Class Libraries (`_content/`) are generated alongside. DocSite also generates `llms.txt` and `_llms/` files.

## Beat 3: Configure the base URL for subdirectory deployment

GitHub Pages project repos serve at `username.github.io/repo-name/`. The build command takes the subdirectory as an argument.

### What to show
- Command: `dotnet run -- build /my-docs/`
- Penn's `BaseUrlRewritingProcessor` rewrites all root-relative URLs in the HTML to include the base path, and adds a `data-base-url` attribute to `<body>` for client-side JavaScript.
- For root-domain deployment (`username.github.io`), simply omit the base URL argument.

### Key points
- The base URL must include both leading and trailing slashes: `/my-docs/`
- Root-relative URLs in HTML (like `/styles.css`) become `/my-docs/styles.css` after rewriting
- The `data-base-url` attribute on `<body>` ensures client-side JavaScript (SPA navigation, search) knows the prefix
- If the site uses `CanonicalBaseUrl` in options (e.g., `P:Penn.DocSite.DocSiteOptions.CanonicalBaseUrl`), the base URL from the build command handles deployment path rewriting separately from the canonical URL used in sitemaps and meta tags

## Beat 4: Create the GitHub Actions workflow

The reader creates a complete `.github/workflows/deploy.yml` file. The goal is a copy-paste-ready workflow that handles both root and subdirectory deployment.

### What to show
- Create `.github/workflows/deploy.yml` with the complete workflow:
  - **Trigger**: `on: push: branches: [main]`
  - **Permissions**: `pages: write`, `id-token: write`, `contents: read`
  - **Job: build-and-deploy**:
    1. `actions/checkout@v4`
    2. `actions/setup-dotnet@v4` with `dotnet-version: '11.0.x'`
    3. `dotnet restore`
    4. `dotnet run --project <path-to-project> -- build /${{ github.event.repository.name }}/` -- uses the repo name as the base URL for subdirectory deployment
    5. `touch output/.nojekyll` -- creates the `.nojekyll` file
    6. `actions/configure-pages@v5`
    7. `actions/upload-pages-artifact@v3` with `path: output`
    8. `actions/deploy-pages@v4`
- Explain the `.nojekyll` file: GitHub Pages processes sites through Jekyll by default, which ignores files/directories starting with `_` (like `_content/`). The `.nojekyll` file disables this processing.
- Note: the workflow uses `${{ github.event.repository.name }}` to automatically determine the subdirectory path -- for a repo named `my-docs`, this becomes `/my-docs/`

### Key points
- The `id-token: write` permission is required by `actions/deploy-pages` for OIDC-based deployment
- The `pages: write` permission allows the workflow to publish to GitHub Pages
- `actions/configure-pages` sets up the GitHub Pages deployment target
- `actions/upload-pages-artifact` packages the output directory as a GitHub Pages artifact
- `actions/deploy-pages` deploys the artifact to GitHub Pages
- The `dotnet run --project <path>` syntax is needed when the project is not in the repository root -- replace `<path>` with the actual project path (e.g., `docs/Penn.Docs`)
- For monorepo setups, the reader may need `dotnet restore` before `dotnet run` to ensure all dependencies are available

## Beat 5: Enable GitHub Pages in repository settings

The reader configures the repository to deploy from GitHub Actions. The goal is the one-time setup step that connects the workflow to GitHub Pages.

### What to show
- Navigate to the repository on GitHub
- Go to Settings, then Pages (in the sidebar under "Code and automation")
- Under "Source", select "GitHub Actions" from the dropdown
- Push the workflow file to the `main` branch
- Navigate to the Actions tab and watch the workflow run
- When complete, the Pages URL appears in the repository settings under Pages

### Key points
- GitHub Pages must be enabled before the first deployment -- without this setting, the deploy step will fail
- The "GitHub Actions" source option is different from the legacy "Deploy from a branch" option -- it uses the `actions/deploy-pages` action rather than watching a specific branch
- The first deployment may take a minute or two to propagate
- The deployed URL follows the pattern: `https://<username>.github.io/<repo-name>/` for project repos, or `https://<username>.github.io/` for the special `<username>.github.io` repo

## Beat 6: Verify the deployment

The reader visits the deployed URL and checks that everything works. The goal is a checklist that catches the most common deployment issues.

### What to show
- Visit `https://<username>.github.io/<repo-name>/` (or the root domain for `*.github.io` repos)
- Verification checklist:
  1. Home page loads with correct styling (MonorailCSS stylesheet loaded)
  2. Navigation links work (sidebar clicks navigate correctly)
  3. Search works (Ctrl+K opens modal, results appear)
  4. Images and static assets load (no broken image icons)
  5. Dark mode toggle works
  6. 404 page shows when visiting a non-existent URL
  7. If using SPA navigation, page transitions are smooth (no full reloads)
  8. If using DocSite, the outline nav tracks headings on scroll
  9. RSS feed loads at `/feed.xml` (BlogSite only)
  10. Sitemap loads at `/sitemap.xml`

### Key points
- If the page loads but looks unstyled, the base URL is likely wrong -- check that `/styles.css` returns CSS, not a 404
- Browser DevTools (Network tab) is the fastest way to find broken asset URLs -- look for 404 responses
- The `data-base-url` attribute on `<body>` should match the deployment path -- inspect it in DevTools to verify
- For troubleshooting deployment issues (missing styles, broken search, 404s), see the Deploying to a Subdirectory how-to.

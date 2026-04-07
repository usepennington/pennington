---
title: "Deploying to GitHub Pages"
description: "Generate a static site and deploy it to GitHub Pages using GitHub Actions"
uid: "penn.getting-started.deploying-to-github-pages"
order: 1090
---

Penn runs as a normal ASP.NET application in development. For deployment, it switches to static generation mode: it starts the server, crawls every page, writes HTML files to an output directory, and shuts down. The result is a folder of static files that GitHub Pages can serve directly.

## What You'll Learn

- How Penn's `build` command generates a static site
- How to configure GitHub Pages with GitHub Actions
- How to set up a workflow that builds and deploys automatically
- How to read the build report and fix common issues

## Prerequisites

- A GitHub account and repository
- Completed the [Creating Your First Site](xref:penn.getting-started.creating-first-site) tutorial
- Your Penn project pushed to a GitHub repository

<Steps>
<Step stepNumber="1">
## Understand the Build Command

Penn's static generation is triggered by passing `build` as a command-line argument:

```bash
dotnet run -- build
```

The `RunOrBuildAsync` method checks for this argument. When present, it starts the server internally, crawls every discovered page, writes the output, and exits. Without it, the application runs as a normal dev server.

You can pass two optional positional arguments after `build`:

```bash
dotnet run -- build "/repository-name/"
dotnet run -- build "/repository-name/" "dist"
```

| Argument | Position | Default | Description |
|----------|----------|---------|-------------|
| Base URL | 1st | `/` | Path prefix for all links and assets |
| Output directory | 2nd | `output` | Where generated files are written |

The base URL is important for GitHub Pages. Your site is served at `https://username.github.io/repository-name/`, not at the root. Penn rewrites all links, asset paths, and navigation URLs to include this prefix during static generation. In development, everything works at `/` as usual.

> [!NOTE]
> The base URL and output directory are positional CLI arguments only. There is no environment variable equivalent.
</Step>

<Step stepNumber="2">
## Prepare Your Repository

### Enable GitHub Pages

1. Navigate to your repository on GitHub
2. Go to **Settings** > **Pages**
3. Under **Source**, select **GitHub Actions**

This tells GitHub Pages to deploy from a workflow artifact instead of a branch.

### Pin the .NET SDK Version

Create a `global.json` in your repository root:

```json
{
  "sdk": {
    "version": "11.0.0",
    "rollForward": "latestMinor"
  }
}
```

This ensures the GitHub Actions runner uses the same .NET SDK version as your local environment. The `rollForward` setting allows patch updates without breaking the build.

### Verify Project Structure

Your repository should look something like this:

```
your-repo/
├── .github/
│   └── workflows/
│       └── deploy.yml
├── src/
│   └── YourDocsSite/
│       ├── YourDocsSite.csproj
│       ├── Program.cs
│       └── Content/
├── global.json
└── README.md
```

The exact layout depends on your solution. The workflow just needs to know the path to your project.
</Step>

<Step stepNumber="3">
## Create the GitHub Actions Workflow

Create `.github/workflows/deploy.yml`:

```yaml
name: Build and deploy to GitHub Pages

on:
  push:
    branches: [ "*" ]
  pull_request:
    branches: [ "main" ]

env:
  ASPNETCORE_ENVIRONMENT: Production
  WEBAPP_PATH: ./src/YourDocsSite/
  WEBAPP_CSPROJ: YourDocsSite.csproj

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: "pages"
  cancel-in-progress: false

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Install .NET
        uses: actions/setup-dotnet@v4
        with:
          global-json-file: global.json

      - name: Build and generate static files
        env:
          DOTNET_CLI_TELEMETRY_OPTOUT: true
        run: |
          dotnet build
          dotnet run --project ${{ env.WEBAPP_PATH }}${{ env.WEBAPP_CSPROJ }} --configuration Release -- build "/your-repository-name/"

      - name: Setup Pages
        uses: actions/configure-pages@v4

      - name: Add .nojekyll file
        run: touch ${{ env.WEBAPP_PATH }}output/.nojekyll

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: ${{ env.WEBAPP_PATH }}output

  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    needs: build
    if: (github.event_name == 'push' && github.ref == 'refs/heads/main') || (github.event_name == 'pull_request' && github.event.action == 'closed' && github.event.pull_request.merged == true)
    steps:
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

### What to Customize

Three values need your actual project details:

- **`WEBAPP_PATH`** -- path to your doc site project directory (e.g., `./docs/MyDocs/`)
- **`WEBAPP_CSPROJ`** -- your project file name (e.g., `MyDocs.csproj`)
- **`"/your-repository-name/"`** -- your GitHub repository name, used as the base URL

The `.nojekyll` file tells GitHub Pages not to process your output through Jekyll. Without it, directories starting with underscores get silently ignored.

### How the Workflow Runs

- **Build job** runs on every push and on pull requests to `main`. This gives you CI validation on feature branches.
- **Deploy job** only runs when code lands on `main` (direct push or merged PR). Feature branches build but don't deploy.
</Step>

<Step stepNumber="4">
## Deploy and Verify

Commit and push:

```bash
git add .github/workflows/deploy.yml global.json
git commit -m "Add GitHub Pages deployment"
git push origin main
```

Then:

1. Go to the **Actions** tab in your repository
2. Watch the workflow run
3. Once the deploy job completes, your site is live at `https://username.github.io/repository-name/`

The first deployment takes a minute or two. Subsequent runs are faster once the runner caches the .NET SDK and NuGet packages.

Verify that:

- Pages load with styling intact
- Navigation works between pages
- Images and assets resolve correctly
</Step>

<Step stepNumber="5">
## Custom Domain (Optional)

With a custom domain, your base URL changes from `/repository-name/` to `/`.

### Configure DNS

Add a CNAME record pointing to your GitHub Pages site:

```
CNAME  docs.yourdomain.com  username.github.io
```

### Update Repository Settings

1. Go to **Settings** > **Pages**
2. Enter your custom domain in the **Custom domain** field
3. Enable **Enforce HTTPS** once the certificate provisions

### Update the Build Command

Change the base URL in your workflow to `/`:

```yaml
- name: Build and generate static files
  env:
    DOTNET_CLI_TELEMETRY_OPTOUT: true
  run: |
    dotnet build
    dotnet run --project ${{ env.WEBAPP_PATH }}${{ env.WEBAPP_CSPROJ }} --configuration Release -- build "/"
```

No subdirectory prefix is needed when serving from a custom domain.
</Step>
</Steps>

## Reading the Build Report

When the build completes, Penn prints a summary to the console:

```
Build Complete — 32 pages in 2.4s
  32 pages generated
  1 warnings
```

The report includes:

- **Page count** -- total pages generated, skipped (drafts), and failed
- **Warnings** -- non-fatal issues such as missing images or unresolved xrefs
- **Broken links** -- internal links that point to pages that don't exist, listed with the source page and target URL
- **Errors** -- fatal problems that prevented page generation

Penn generates a `404.html` page automatically during the build (Phase 8 of the generation process). GitHub Pages serves this file for any URL that doesn't match a generated page.

The build also runs link verification (Phase 9), checking every internal link across all generated HTML. If broken links are found, they appear in the report:

```
WARNINGS
  2 broken links found:
    /getting-started links to /nonexistent-page (page not found)
    /guides/intro links to /old-path (page not found)
```

> [!IMPORTANT]
> The build sets a non-zero exit code when errors or broken links are detected. This causes the GitHub Actions workflow to fail, preventing deployment of a broken site. Fix all reported issues before merging to `main`.

## Troubleshooting

**Site loads but CSS, JS, or images are missing**
: The base URL in your build command must exactly match your GitHub repository name, including case. `build "/My-Repo/"` and `build "/my-repo/"` produce different asset paths.

**404 on all pages after deployment**
: Ensure the `.nojekyll` file step is in your workflow. GitHub Pages' default Jekyll processing ignores directories starting with underscores, which breaks static file serving.

**Workflow runs but site doesn't update**
: The deploy job only runs on pushes to `main`. Check the Actions tab -- the build job succeeded on your feature branch, but the deploy job was skipped. This is by design.

**Build succeeds locally but fails in CI**
: Check your `global.json`. If it pins a .NET version that isn't available on the GitHub runner, `setup-dotnet` fails. Use `rollForward: "latestMinor"` for flexibility.

**Build exits with a non-zero code**
: Read the build report output. Broken internal links and generation errors both cause a non-zero exit. The report lists every broken link with its source page and target URL so you can fix them directly.

## Next Steps

- [Using UI Elements](xref:penn.getting-started.using-ui-elements) -- enhance your pages with cards, badges, steps, and more
- [Connecting to Roslyn](xref:penn.getting-started.connecting-to-roslyn) -- embed live, verified code examples from your .NET solution

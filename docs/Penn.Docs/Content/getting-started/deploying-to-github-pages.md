---
title: "Deploying to GitHub Pages"
description: "Generate a static site and deploy it to GitHub Pages with a GitHub Actions workflow"
uid: "penn.getting-started.deploying-to-github-pages"
order: 1090
---

Penn runs as a normal ASP.NET application in development. For deployment, it switches to static generation mode: it starts the server, crawls every page, writes HTML files to an output directory, and shuts down. The result is a folder of static files that any web server — or GitHub Pages — can serve.

No server runtime required. No containers. Just files. The web's most underrated deployment model.

## Prerequisites

- A GitHub account and repository
- Completed at least the [Creating Your First Site](xref:penn.getting-started.creating-first-site) tutorial
- Your Penn project pushed to a GitHub repository
- Basic familiarity with Git

<Steps>
<Step stepNumber="1">
## Understand the Build Command

Penn's static generation is triggered by passing `build` as a command-line argument:

```bash
dotnet run -- build
```

This starts the server, requests every known page, writes the HTML to an `output` directory, and exits. You can also specify a base URL and output directory:

```bash
dotnet run -- build "/repository-name/"
dotnet run -- build "/repository-name/" "dist"
```

**Arguments:**
- `build` — triggers static generation instead of the dev server
- Base URL (optional) — the path prefix for all links, e.g., `/my-project/` for GitHub Pages subdirectory hosting. Defaults to `/`.
- Output directory (optional) — where to write generated files. Defaults to `output`.

The base URL matters because GitHub Pages serves your site at `https://username.github.io/repository-name/`, not at the root. Penn rewrites all links, asset paths, and navigation URLs to include this prefix during static generation. In development, everything works at `/` as usual.

You can also set the base URL via the `BaseHref` environment variable if you'd rather not pass it as an argument:

```bash
export BaseHref="/my-project/"
dotnet run -- build
```
</Step>

<Step stepNumber="2">
## Prepare Your Repository

### Enable GitHub Pages

1. Navigate to your repository on GitHub
2. Go to **Settings** > **Pages**
3. Under **Source**, select **GitHub Actions**

### Pin the .NET SDK Version

Create a `global.json` in your repository root to ensure consistent builds:

```json
{
  "sdk": {
    "version": "11.0.0",
    "rollForward": "latestMinor"
  }
}
```

This ensures GitHub Actions uses the same .NET SDK version as your local development environment. Without it, you're rolling the dice on which SDK version the runner has cached.

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

The exact structure depends on your solution layout. The workflow just needs to know where your project lives.
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

Three things need your actual values:

- **`WEBAPP_PATH`** — path to your doc site project directory (e.g., `./docs/MyDocs/`)
- **`WEBAPP_CSPROJ`** — your project file name (e.g., `MyDocs.csproj`)
- **`"/your-repository-name/"`** — your GitHub repository name in the build command

The `.nojekyll` file tells GitHub Pages not to process your output through Jekyll. Without it, files and directories starting with underscores get silently ignored, which breaks things in ways that are deeply annoying to debug.

### How the Workflow Runs

- **Build job** runs on every push to any branch and on pull requests to `main`. This gives you CI validation on feature branches.
- **Deploy job** only runs when code lands on `main` (direct push or merged PR). Your site only updates on main — feature branches build but don't deploy.
</Step>

<Step stepNumber="4">
## Deploy

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

The first deployment takes a minute or two. Subsequent deployments are faster since the runner caches the .NET SDK and NuGet packages.
</Step>

<Step stepNumber="5">
## Custom Domain (Optional)

If you have a custom domain, the base URL changes from `/repository-name/` to `/`:

### Configure DNS

Add a CNAME record pointing to your GitHub Pages site:

```
CNAME  www.yourdomain.com  username.github.io
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

With a custom domain, your site lives at the root — no subdirectory prefix needed.
</Step>
</Steps>

## What Success Looks Like

After pushing to `main`, navigate to the **Actions** tab and watch the workflow. Once the deploy job finishes (typically 1-2 minutes), your site is live.

Verify that:
- Pages load with styling intact
- Navigation works between pages
- Images and assets load correctly
- If using Roslyn, code blocks render with syntax highlighting

Every push to `main` from this point forward triggers an automatic rebuild and deploy. Push content changes and they're live in minutes.

## Troubleshooting

**Site loads but CSS/JS/images are missing**
: The repository name in your build command (`build "/your-repository-name/"`) must exactly match your GitHub repository name, including case. A mismatch means every asset URL is wrong.

**404 on all pages after deployment**
: Make sure the `.nojekyll` file step is in your workflow. GitHub Pages' default Jekyll processing ignores directories that start with underscores, which breaks static file serving.

**Workflow runs but site doesn't update**
: The deploy job only runs on pushes to `main`. Check the Actions tab — you'll see the build job succeeded on your branch, but the deploy job was skipped. This is by design.

**Build succeeds locally but fails in CI**
: Check your `global.json`. If it pins a .NET version that isn't available on the GitHub runner, `setup-dotnet` will fail. Use `rollForward: "latestMinor"` to give yourself some flexibility.

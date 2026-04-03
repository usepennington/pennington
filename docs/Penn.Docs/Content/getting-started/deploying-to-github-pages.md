---
title: "Deploying to GitHub Pages"
description: "Configure and deploy your MyLittleContentEngine site to GitHub Pages with automated builds"
uid: "docs.getting-started.deploying-to-github-pages"
order: 1090
---

This tutorial covers:

- Setting up GitHub Actions for automated builds
- Configuring base URLs for subdirectory deployment
- Handling static assets and routing correctly
- Custom domain configuration

## Prerequisites

- A GitHub account and repository
- Completed at least the ["Creating Your First Site"](xref:docs.getting-started.creating-first-site) tutorial
- Basic understanding of Git and GitHub
- Your MyLittleContentEngine project pushed to a GitHub repository

<Steps>
<Step stepNumber="1">
## Prepare Your Repository

First, ensure your project is properly configured for GitHub Pages deployment.

### Repository Settings

1. Navigate to your repository on GitHub
2. Go to **Settings** → **Pages**
3. Under **Source**, select **GitHub Actions**

### Project Structure

Make sure your project structure follows this pattern:

```
your-repo/
├── .github/
│   └── workflows/
│       └── deploy.yml
├── src/
│   └── YourProject/
│       ├── YourProject.csproj
│       ├── Program.cs
│       └── Content/
├── global.json
└── README.md
```

Note that we are using a [`global.json`](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) file to specify the
.NET SDK version. This ensures consistent builds across different environments, and is used in the GitHub Actions workflow to install
the appropriate .NET version.

Create a `global.json` file in your repository root:

```json
{
  "sdk": {
    "version": "9.0.0",
    "rollForward": "latestMinor"
  }
}
```

This ensures GitHub Actions uses the same .NET version as your local development environment.
</Step>
<Step stepNumber="2">
## Configure GitHub Actions Workflow

Create `.github/workflows/deploy.yml` in your repository root:

```yaml
name: Build and publish to GitHub Pages

on:
  push:
    branches: [ "*" ]
  pull_request:
    branches: [ "main" ]

env:
  ASPNETCORE_ENVIRONMENT: Production
  WEBAPP_PATH: ./src/YourProject/
  WEBAPP_CSPROJ: YourProject.csproj

permissions:
  contents: read
  pages: write
  id-token: write

# Allow only one concurrent deployment
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

      - name: Run webapp and generate static files
        env:
          DOTNET_CLI_TELEMETRY_OPTOUT: true
        run: |
          dotnet build
          dotnet run --project ${{ env.WEBAPP_PATH }}${{env.WEBAPP_CSPROJ}} --configuration Release -- build "/your-repository-name/"

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

### Key Configuration Points

**Variables to Update:**

- `WEBAPP_PATH`: Path to your project directory
- `WEBAPP_CSPROJ`: Your project file name
- `"/your-repository-name/"`: Your repository name in the build command (for GitHub Pages subdirectory)

**Important:** Replace `your-repository-name` and `YourProject` with your actual values.
</Step>
<Step stepNumber="3">
## Configuring Base URLs

This is one of the most important aspects of GitHub Pages deployment. Understanding BaseUrl is crucial for your site to
work correctly.

### Why BaseUrl Matters

**Local Development vs GitHub Pages:**

- **Local development**: Your site runs at `http://localhost:5000/` (root domain)
- **GitHub Pages**: Your site runs at `https://username.github.io/repository-name/` (subdirectory)

Without proper BaseUrl configuration, your site will have broken links, missing CSS, and non-functional navigation when
deployed to GitHub Pages.

See the [Linking Documents and Media](xref:docs.guides.linking-documents-and-media) guide for more details on how
MyLittleContentEngine handles links.

### How BaseUrl Configuration Works

MyLittleContentEngine automatically handles BaseUrl configuration through command-line arguments. When you run the build command:

```bash
dotnet run -- build "/your-repository-name/"
```

The framework:
1. Parses the command-line arguments passed to `RunBlogSiteAsync(args)` or `RunDocSiteAsync(args)`
2. Extracts the BaseUrl (`/your-repository-name/`) from the second argument after `build`
3. Configures `OutputOptions` internally with this BaseUrl
4. Uses this BaseUrl to rewrite all links, assets, and navigation URLs during static generation

**You don't need to modify your `Program.cs`** - the BlogSite and DocSite packages handle this automatically when you pass `args` to `RunBlogSiteAsync(args)` or `RunDocSiteAsync(args)`.

### Command-Line Build Format

```bash
dotnet run --project YourProject.csproj -- build [BaseUrl] [OutputFolder]
```

**Arguments:**
- `build` - Triggers static generation mode
- `BaseUrl` (optional) - The base URL path (e.g., `/repository-name/` for GitHub Pages, `/` for root deployment)
- `OutputFolder` (optional) - Output directory path (defaults to `output`)

**Examples:**
```bash
# GitHub Pages subdirectory deployment
dotnet run -- build "/my-blog/"

# Root deployment (custom domain)
dotnet run -- build "/"

# Custom output folder
dotnet run -- build "/my-blog/" "dist"
```

**Environment Variable Fallback:**

If no BaseUrl is provided via command-line, the system checks for a `BaseHref` environment variable:

```bash
export BaseHref="/my-blog/"
dotnet run -- build
```
</Step>
<Step stepNumber="4">

## Test Your Deployment

### Push Your Changes

Commit and push your workflow file:

```bash
git add .github/workflows/deploy.yml
git commit -m "Add GitHub Pages deployment workflow"
git push origin main
```

### Monitor the Build

1. Go to the **Actions** tab in your repository
2. Watch the workflow run
3. Check for any errors in the build process

### Verify Deployment

Once the workflow completes:

1. Your site should be available at `https://username.github.io/repository-name/`
2. Check that all pages load correctly
3. Verify that navigation works properly
4. Test that images and other assets load

</Step>
<Step stepNumber="5">
## Custom Domain (Optional)

To use a custom domain:

### Configure DNS

Add a CNAME record pointing to `username.github.io`:

```
CNAME  www.yourdomain.com  username.github.io
```

### Update Repository Settings

1. Go to **Settings** → **Pages**
2. Enter your custom domain in the **Custom domain** field
3. GitHub will automatically create a `CNAME` file in your repository

### Update Base URL

Modify your workflow to use your custom domain:

```yaml
- name: Run webapp and generate static files
  env:
    DOTNET_CLI_TELEMETRY_OPTOUT: true
  run: |
    dotnet build
    dotnet run --project ${{ env.WEBAPP_PATH }}${{env.WEBAPP_CSPROJ}} --configuration Release -- build "/"  # Root path for custom domain
```
</Step>
</Steps>



## What Success Looks Like

After pushing to `main`, navigate to the **Actions** tab in your repository and watch the workflow run. Once the
deploy job completes (typically 1–2 minutes), your site will be live at
`https://username.github.io/repository-name/`.

Verify that:
- Pages load correctly with styling applied
- Navigation links work between pages
- Images and other assets load

Every time you push to the `main` branch from this point forward, your site rebuilds and deploys automatically.

## Troubleshooting

**Site loads but assets are missing (CSS, JS, images)**
: Check that the repository name in your build command (`build "/your-repository-name/"`) exactly matches your
GitHub repository name, including case.

**404 on all pages after deployment**
: Ensure the `.nojekyll` file step is present in your workflow. Without it, GitHub Pages' Jekyll processing
interferes with the generated HTML.

**Workflow runs but site isn't updating**
: The deploy job only runs on pushes to `main`. Check the **Actions** tab to confirm the *deploy* job ran,
not just the *build* job. The build job runs on all branches for CI validation.
---
title: "Deploying to Subdirectories"
description: "Configure BaseUrl rewriting so your Penn site works at a non-root path"
uid: "penn.guides.deploying-to-subdirectories"
order: 2200
---

When you deploy a Penn site to GitHub Pages, Azure Static Web Apps, or any host that serves your site under a path prefix like `/my-project/`, every root-relative URL in the generated HTML needs that prefix. A link written as `/guides/search` must become `/my-project/guides/search` in the output. Penn handles this automatically through the `BaseUrlRewritingProcessor`, a response processor that rewrites URLs at build time based on the base URL you pass on the command line.

This guide explains how the rewriting works, how to configure it, and how to set up CI/CD for subdirectory deployments.

## How BaseUrl Rewriting Works

### The Problem

During development, your Penn site runs at `http://localhost:5000/`. Links like `/guides/search` resolve correctly because the site occupies the root path. When you deploy to `https://example.github.io/my-project/`, that same link points to `https://example.github.io/guides/search` -- a page that doesn't exist. Every `href`, `src`, and `action` attribute that starts with `/` breaks.

You could manually prefix every URL in your content with the deployment path, but that ties your content to one specific hosting arrangement. Move the site to a different repository or a custom domain and every link breaks again.

Penn solves this by rewriting root-relative URLs during static site generation. You write canonical paths starting with `/`. The `BaseUrlRewritingProcessor` prepends the base URL to each one in the rendered HTML.

### The IResponseProcessor Pipeline

`BaseUrlRewritingProcessor` implements `IResponseProcessor`, the interface that Penn's `ResponseProcessingMiddleware` uses to transform HTTP response bodies. The middleware captures the response, filters processors through `ShouldProcess`, sorts them by `Order`, and chains their `ProcessAsync` calls sequentially.

Penn registers four processors in `AddPenn()`:

| Processor | Order | Purpose |
|-----------|-------|---------|
| `XrefResolvingProcessor` | -10 | Resolves `xref:` links to canonical paths |
| `BaseUrlRewritingProcessor` | 0 | Prepends base URL to root-relative paths |
| `LiveReloadScriptProcessor` | 1000 | Injects live reload WebSocket script |
| `DiagnosticOverlayProcessor` | 10000 | Adds diagnostic overlay in development |

`BaseUrlRewritingProcessor` runs after cross-reference resolution and before everything else. This ordering matters: `XrefResolvingProcessor` produces root-relative URLs from `xref:` tags, and then `BaseUrlRewritingProcessor` rewrites those URLs along with all the others.

### ShouldProcess Guard

The processor skips work entirely when no rewriting is needed:

- If the base URL is empty or `"/"`, no rewriting is required. Root deployments incur zero processing overhead.
- If the response status code is outside the 2xx range, the response is not HTML content worth rewriting.
- If the content type is not `text/html` or `application/json`, the response is a static file or API response that doesn't contain URL attributes.

### HTML Rewriting with AngleSharp

When rewriting is needed, `RewriteHtmlAsync` parses the response body with AngleSharp's `IBrowsingContext`. It queries all elements that have `href`, `src`, or `action` attributes, then calls `RewriteAttribute` for each one. `RewriteAttribute` checks whether the attribute value starts with `/` but not `//` (which would be a protocol-relative URL). If the check passes, it prepends the configured base URL.

The processor also sets a `data-base-url` attribute on the `<body>` element. Client-side JavaScript -- such as Penn's SPA navigation scripts -- reads this attribute to construct correct URLs dynamically without hardcoding the deployment path.

## Configuration

### OutputOptions.FromArgs

The base URL comes from command-line arguments, parsed by `OutputOptions.FromArgs`. The argument layout after the `build` command is positional:

| Position | Argument | Default | Description |
|----------|----------|---------|-------------|
| 1st | Base URL | `/` | Path prefix prepended to all root-relative URLs |
| 2nd | Output directory | `output` | Directory where generated files are written |

`AddPenn()` reads `Environment.GetCommandLineArgs()`, strips the executable name by slicing `[1..]`, and passes the result to `OutputOptions.FromArgs`. The resulting `OutputOptions` singleton is registered in DI. `BaseUrlRewritingProcessor` receives it through constructor injection and trims any trailing slash from the base URL.

Common invocations:

```bash
# Build for subdirectory deployment
dotnet run -- build /my-project

# Build for root deployment (default)
dotnet run -- build /

# Build for subdirectory with custom output directory
dotnet run -- build /my-project dist
```

The leading slash is required. Penn normalizes the value, so `/my-project` and `/my-project/` produce the same result.

### With DocSite or Custom Sites

Both `RunDocSiteAsync` and `RunOrBuildAsync` use the same mechanism. The key requirement is that command-line arguments flow through to the entry point method.

**DocSite pattern:**

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "Project documentation",
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

`RunDocSiteAsync` delegates directly to `RunOrBuildAsync`.

**Custom site pattern:**

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Site";
    // ... content sources
});

var app = builder.Build();
app.UsePenn();
await app.RunOrBuildAsync(args);
```

In both cases, `args` from `Main` (or the top-level statement) must pass through unchanged. `RunOrBuildAsync` checks `args[0]` for the `build` command. If present, it starts the application, resolves `OutputGenerationService` from DI, crawls every discovered page, writes the output directory, and exits. If absent, the application runs as a normal dev server.

The `OutputOptions` singleton is already registered by `AddPenn()` before `RunOrBuildAsync` executes, so `BaseUrlRewritingProcessor` has the correct base URL from the moment the first response is processed during the build crawl.

## Example Transformation

Given a base URL of `/my-project`, the processor transforms the following HTML:

**Before:**

```html
<body>
  <a href="/">Home</a>
  <a href="/guides/search">Search Guide</a>
  <img src="/images/logo.png" alt="Logo" />
  <script src="/js/app.js"></script>
  <a href="https://github.com/example/repo">GitHub</a>
  <a href="./sibling-page">Sibling</a>
</body>
```

**After:**

```html
<body data-base-url="/my-project">
  <a href="/my-project/">Home</a>
  <a href="/my-project/guides/search">Search Guide</a>
  <img src="/my-project/images/logo.png" alt="Logo" />
  <script src="/my-project/js/app.js"></script>
  <a href="https://github.com/example/repo">GitHub</a>
  <a href="./sibling-page">Sibling</a>
</body>
```

The external link (`https://...`) and the relative link (`./sibling-page`) are left unchanged. Only root-relative paths starting with `/` are rewritten. The `data-base-url` attribute is added to `<body>` for client-side scripts to read.

## GitHub Actions Workflow

Here is a complete workflow for deploying to GitHub Pages at a subdirectory path. Create `.github/workflows/deploy.yml`:

```yaml
name: Build and deploy to GitHub Pages

on:
  push:
    branches: [ "*" ]
  pull_request:
    branches: [ "main" ]

env:
  ASPNETCORE_ENVIRONMENT: Production
  WEBAPP_PATH: ./src/MyDocsSite/
  WEBAPP_CSPROJ: MyDocsSite.csproj

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

Three values need your project details:

- **`WEBAPP_PATH`**: path to your doc site project directory (e.g., `./docs/MyDocs/`)
- **`WEBAPP_CSPROJ`**: your project file name (e.g., `MyDocs.csproj`)
- **`"/your-repository-name/"`**: your GitHub repository name, used as the base URL argument

The `.nojekyll` file prevents GitHub Pages from processing your output through Jekyll. Without it, directories starting with underscores are silently ignored.

The build job runs on every push for CI validation. The deploy job only runs when code lands on `main`, preventing feature branches from overwriting the live site.

## Testing Locally

Before deploying, verify that subdirectory rewriting works by building locally with a test base URL:

```bash
dotnet run -- build /my-project
```

This generates the static site in the `output` directory with all URLs rewritten. Serve the output with any static file server:

```bash
npx serve output
```

Or with Python:

```bash
python -m http.server 8000 --directory output
```

Open the site and verify that:

- Navigation links work and include the `/my-project` prefix
- Images and static assets load correctly
- The browser URL bar shows the expected paths
- SPA navigation (if enabled) works between pages

You can also inspect the HTML source to confirm that `<body data-base-url="/my-project">` is present and that root-relative URLs have been rewritten.

## Best Practices

- **Use root-relative URLs in your content.** Write `/guides/search`, not `guides/search`. The rewriter identifies root-relative URLs by the leading `/` and cannot rewrite relative paths.
- **Pass `args` through to the entry point method.** Both `RunDocSiteAsync(args)` and `RunOrBuildAsync(args)` need the original command-line arguments to detect `build` mode and parse the base URL. Do not filter or modify `args` before passing them.
- **Test with a static file server before deploying.** A build that succeeds locally doesn't guarantee that rewriting is correct. Serve the output directory and click through the site to catch path issues before they reach production.
- **Inspect `data-base-url` when debugging client-side issues.** If SPA navigation or custom JavaScript constructs URLs, read the `data-base-url` attribute from `<body>` instead of assuming a root deployment.
- **Protocol-relative URLs are left alone.** URLs starting with `//` (like `//cdn.example.com/lib.js`) are intentionally not rewritten. They are valid cross-protocol references, not root-relative paths.
- **JSON responses pass the ShouldProcess check but are not rewritten.** The `ShouldProcess` method accepts `application/json` content types, but `ProcessAsync` only rewrites `text/html` responses. This is an intentional design point -- JSON payloads like the SPA navigation data contain paths that are already correct.

## Next Steps

- [Deploying to GitHub Pages](xref:penn.getting-started.deploying-to-github-pages) -- full walkthrough of GitHub Pages deployment including custom domains and reading the build report
- [Link Types and Syntax](xref:penn.guides.linking-documents-and-media) -- how Penn resolves cross-references and media paths that the rewriter transforms
- [Development vs Deployment Architecture](xref:penn.under-the-hood.dev-vs-deployment-architecture) -- how the response processor pipeline differs between dev mode and static build

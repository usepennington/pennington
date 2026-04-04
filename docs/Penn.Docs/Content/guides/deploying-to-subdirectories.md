---
title: "Deploying to Subdirectories"
description: "Configure BaseUrl rewriting so your Penn site works correctly when hosted at a non-root path"
uid: "penn.guides.deploying-to-subdirectories"
order: 2200
---

If your site lives at `https://example.com/my-project/` instead of `https://example.com/`, every root-relative URL in your HTML needs the `/my-project` prefix. Penn's <xref:T:Penn.Infrastructure.BaseUrlRewritingProcessor> handles this automatically -- you just tell it the base path.

This is the kind of problem that's simple to describe and surprisingly annoying to get right by hand. Fortunately, you don't have to.

## How BaseUrl Rewriting Works

The `BaseUrlRewritingProcessor` is an `IResponseProcessor` registered by `AddPenn()`. It inspects HTML and JSON responses and rewrites root-relative URLs to include the configured base path.

Specifically, it rewrites:

- `href="/..."` attributes
- `src="/..."` attributes
- `action="/..."` attributes

It also adds a `data-base-url` attribute to the `<body>` element, which client-side scripts use to resolve paths correctly.

### Example Transformation

With `BaseUrl = "/my-project"`:

```html
<!-- Before -->
<a href="/guides/search">Search Guide</a>
<script src="/scripts/app.js"></script>

<!-- After -->
<a href="/my-project/guides/search">Search Guide</a>
<script src="/my-project/scripts/app.js"></script>
<body data-base-url="/my-project">
```

The processor skips URLs that start with `//` (protocol-relative) and only runs when the base URL is something other than `/`. If you're deploying to the root, it does nothing. It's polite like that.

### Processing Order

`BaseUrlRewritingProcessor` has `Order = 0`, meaning it runs before other response processors like the `CssClassCollectorProcessor` (Order = 100). This ensures CSS class collection sees the final HTML.

## Configuration

### Command Line Arguments

The base URL comes from command line arguments passed to `dotnet run -- build`. <xref:T:Penn.Generation.OutputOptions> parses them:

```csharp
// OutputOptions.FromArgs parsing:
// args[0] = "build" (consumed by RunOrBuildAsync)
// args[1] = base URL (e.g., "/my-project")
// args[2] = output directory (default: "output")
```

So the build command looks like:

```bash
# Deploy to /my-project/ subdirectory
dotnet run -- build /my-project

# Deploy to /my-project/ with custom output directory
dotnet run -- build /my-project custom-output

# Deploy to root (default)
dotnet run -- build /
```

### With DocSite or Custom Sites

If you're using `AddDocSite()`, pass `args` through to `RunDocSiteAsync()` and it handles everything:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(_ => new DocSiteOptions
{
    SiteTitle = "My Documentation",
    Description = "Docs for my project",
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args); // args flow through to RunOrBuildAsync
```

For a custom site using `AddPenn()` directly:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Site";
    // ...
});

var app = builder.Build();
app.UsePenn();
await app.RunOrBuildAsync(args);
```

In both cases, `RunOrBuildAsync()` checks if `args[0]` is `"build"`. If so, it starts the app, runs <xref:T:Penn.Generation.OutputGenerationService> to crawl every discovered page, and writes the output. If not, it runs the app normally for development.

### BaseUrl Guidelines

- **Include leading slash**: `/my-project`, not `my-project`
- **No trailing slash needed**: Penn normalizes it internally
- **Root deployment**: Use `/` or omit the argument entirely

## GitHub Actions Example

Here's a complete workflow for deploying to GitHub Pages at a subdirectory:

```yaml
name: Build and deploy to GitHub Pages

on:
  push:
    branches: [main]

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

      - name: Build and generate static site
        run: |
          dotnet build
          dotnet run --project ${{ env.WEBAPP_PATH }}${{ env.WEBAPP_CSPROJ }} \
            --configuration Release -- build "/your-repository-name/"

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
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

Replace `/your-repository-name/` with your actual GitHub repository name.

## Testing Locally

You can test subdirectory behavior without deploying:

```bash
dotnet run -- build /my-project
```

Then serve the `output` directory with any static file server and verify links resolve correctly. Penn writes redirect stubs for pages that return 301, so even redirects work in the static output.

## Best Practices

1. **Always use root-relative URLs** in your content (`/page`, not `page`). The rewriter only transforms root-relative paths.
2. **Pass `args` through** to `RunOrBuildAsync()` or `RunDocSiteAsync()`. If you forget, the base URL defaults to `/` and your subdirectory links will break.
3. **Test before deploying** with a local static file server. Broken links in production are embarrassing.
4. **Check `data-base-url`** in the rendered HTML if client-side scripts aren't finding assets. The attribute should appear on `<body>`.

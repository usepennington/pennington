---
title: "Deploying to Subdirectories"
description: "Deploy your MyLittleContentEngine site to subdirectories with BaseUrl rewriting middleware"
uid: "docs.guides.deploying-to-subdirectories"
order: 2200
---

When deploying your MyLittleContentEngine site to hosting services like GitHub Pages, Azure Static Web Apps, or any
other host where your site runs in a subdirectory (e.g., `https://mysite.com/my-app/`), you need to configure BaseUrl
rewriting to ensure all links work correctly.

MyLittleContentEngine includes the `BaseUrlRewritingMiddleware` that automatically handles this by rewriting
root-relative URLs in your HTML responses to include the configured base path. You can configure the BaseUrl either
through command line arguments during build or programmatically in your application.

> [!NOTE]
> For comprehensive information about link types, BaseUrl concepts, and testing strategies,
> see <xref:docs.guides.linking-documents-and-media>.

## How BaseUrl Rewriting Works

The BaseUrlRewritingMiddleware performs two main functions:

1. **Cross-Reference Resolution**: Resolves `xref:` links to their actual targets
2. **BaseUrl Rewriting**: Rewrites root-relative URLs (starting with `/`) to include your configured BaseUrl

### URL Rewriting Process

The middleware processes HTML responses and rewrites URLs in the following elements:

- **Links**: `<a href="/page">` and `<link href="/styles.css">`
- **Images**: `<img src="/image.jpg">`
- **Scripts**: `<script src="/script.js">`
- **Media**: `<iframe>`, `<embed>`, `<source>`, `<track>` tags
- **Data attributes**: Any `data-*` attributes containing URLs
- **CSS**: `url()` functions and `@import` statements in style attributes

### Example Transformation

With `BaseUrl = "/my-app/"`, the middleware transforms:

```html
<!-- Before -->
<a href="/docs/getting-started">Getting Started</a>
<img src="/images/logo.png" alt="Logo">
<script src="/scripts/app.js"></script>

<!-- After -->
<a href="/my-app/docs/getting-started">Getting Started</a>
<img src="/my-app/images/logo.png" alt="Logo">
<script src="/my-app/scripts/app.js"></script>
```

## Configuration

<Steps>
<Step stepNumber="1">
## Configure BaseUrl Using Command Line Arguments

The recommended approach is to configure BaseUrl using command line arguments during the build process. This allows you
to specify different BaseUrl values for different deployment environments without modifying your code.

### Using Build Command Arguments

```bash
# For subdirectory deployment
dotnet run -- build "/my-app/"

# For custom output directory
dotnet run -- build "/my-app/" "custom-output"

# For root deployment
dotnet run -- build "/"
```

### For BlogSite and DocSite (Automatic)

If you're using BlogSite or DocSite packages, BaseUrl configuration is handled automatically when you pass `args` to `RunBlogSiteAsync` or `RunDocSiteAsync`:

```csharp
// BlogSite - BaseUrl handled automatically via args
builder.Services.AddBlogSite(_ => new BlogSiteOptions
{
    SiteTitle = "My Blog",
    Description = "My awesome blog",
});

var app = builder.Build();
app.UseBlogSite();
await app.RunBlogSiteAsync(args); // args are used internally for BaseUrl

// DocSite - BaseUrl handled automatically via args
builder.Services.AddDocSite(_ => new DocSiteOptions
{
    SiteTitle = "My Documentation",
    Description = "My awesome docs",
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args); // args are used internally for BaseUrl
```

The `args` parameter is passed through to the framework's `RunOrBuildContent` method, which automatically parses the command-line arguments and configures `OutputOptions` with the appropriate BaseUrl.

### For Custom Sites

If you're building a custom site without BlogSite/DocSite packages:

```csharp
builder.Services.AddContentEngineService(_ => new ContentEngineOptions
{
    SiteTitle = "My Site",
    SiteDescription = "My awesome site",
    ContentRootPath = "Content",
});
```

### BaseUrl Guidelines

- **Include leading slash**: Use `/my-app/`, not `my-app/`
- **Include trailing slash**: Use `/my-app/`, not `/my-app`
- **Root deployment**: Use `/` for root directory deployment
- **Command line takes precedence**: Command line arguments override environment variables
  </Step>

<Step stepNumber="2">
## Configure Static Generation with GitHub Actions

For static site generation, use command line arguments in your build process. The examples below work for both BlogSite/DocSite and custom implementations:

### GitHub Actions Example

```yaml
name: Build and publish to GitHub Pages

on:
  push:
    branches: [ "*" ]
  pull_request:
    branches: [ "main" ]

env:
  ASPNETCORE_ENVIRONMENT: Production
  WEBAPP_PATH: ./src/MyContentSite/
  WEBAPP_CSPROJ: MyContentSite.csproj

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

      - name: Build the Project
        run: |
          dotnet build

      - name: Run webapp and generate static files
        run: |
          dotnet run --project ${{ env.WEBAPP_PATH }}${{env.WEBAPP_CSPROJ}} --configuration Release -- build "/your-repository-name/"

      - name: Setup Pages
        uses: actions/configure-pages@v4

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

  </Step>
  </Steps>

## Cross-Reference Resolution

The BaseUrlRewritingMiddleware also handles cross-reference (`xref:`) resolution, converting references like:

```markdown
See the [ContentService documentation](xref:MyLittleContentEngine.ContentService)
```

Into actual links:

```html
<a href="/my-app/api/MyLittleContentEngine.ContentService">ContentService documentation</a>
```

### Unresolved Cross-References

When a cross-reference cannot be resolved, the middleware:

1. **Preserves the link text** for user experience
2. **Adds error styling** with red color and strikethrough
3. **Includes debug attributes** for troubleshooting:
    - `data-xref-error="Reference not found"`
    - `data-xref-uid="the-unresolved-uid"`

```html
<!-- Unresolved xref becomes: -->
<span data-xref-error="Reference not found"
      data-xref-uid="Unknown.Type"
      class="text-red-500 line-through">
  Unknown Type
</span>
```

## Best Practices

1. **Use command line arguments** for BaseUrl to support multiple deployment targets
2. **Pass args to Run methods** - BlogSite/DocSite automatically handle BaseUrl when you pass `args` to `RunBlogSiteAsync(args)` or `RunDocSiteAsync(args)`
3. **Use root-relative URLs** in your content (`/page` not `page`)
4. **Test locally** with different BaseUrl values using: `dotnet run -- build "/test-path/"`
5. **Monitor for unresolved xrefs** using browser developer tools to check for error attributes
6. **Use LinkService** in Blazor components for consistent URL generation

> [!TIP]
> For guidance on choosing the right link types and linking best practices, see
> the [Linking Documents and Media](xref:docs.guides.linking-documents-and-media) guide.

The BaseUrlRewritingMiddleware makes subdirectory deployment seamless while maintaining the flexibility to deploy to
different environments without code changes.
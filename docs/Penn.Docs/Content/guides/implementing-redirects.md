---
title: "Implementing Redirects"
description: "Set up redirects using front matter properties or configuration files for content reorganization and migration"
uid: "docs.guides.implementing-redirects"
order: 2050
---

MyLittleContentEngine supports redirecting old URLs to new locations when you reorganize content, rename pages, or migrate from another platform.

## Choose Your Approach

**Front Matter Redirects** - Add `redirect_url` to a single markdown file's YAML front matter. Use when:
- Moving or renaming individual pages
- Deprecating specific content
- Keeping the original markdown file in your content structure

**Configuration File Redirects** - Create a `_redirects.yml` file to manage multiple redirects. Use when:
- Bulk migrations from another platform
- Redirecting URLs without corresponding markdown files
- Managing redirects centrally in one file

You can combine both approaches in the same project.

## Redirect a Single Page with Front Matter

<Steps>
<Step stepNumber="1">

### Add redirect_url to Your Markdown File

Open the markdown file you want to redirect and add the `redirect_url` property to its YAML front matter:

```yaml
---
title: "Old Page Title"
redirect_url: /new-location
---
```

The `redirect_url` accepts three URL types:

**Relative URLs** within the same directory:
```yaml
redirect_url: page-one
```

**Absolute URLs** to any path on your site:
```yaml
redirect_url: /docs/guides/new-guide
```

**External URLs** to different domains:
```yaml
redirect_url: https://external-site.com/resource
```

</Step>

<Step stepNumber="2">

### Build Your Site

Run your build command as usual:

```bash
dotnet run
```

MyLittleContentEngine generates an HTML redirect file at the original location instead of rendering the markdown content. The page is automatically excluded from your table of contents.

</Step>
</Steps>

> [!NOTE]
> Any markdown content in the file is ignored when `redirect_url` is set. The file only generates a redirect page.

## Redirect Multiple Pages with Configuration File

<Steps>
<Step stepNumber="1">

### Create _redirects.yml in Content Root

Create a file named `_redirects.yml` in your content root directory (the path you specified in `ContentEngineOptions.ContentRootPath`):

```yaml
redirects:
  /old-page: /new-page
  /archived: https://archive.example.com
  /docs/v1/guide: /docs/v2/guide
```

Each line maps a source path to a destination URL.

</Step>

<Step stepNumber="2">

### Organize Redirects with Comments

Group related redirects and document why they exist:

```yaml
# Site restructuring - moved widgets to console section
redirects:
  /widgets/panel: /console/widgets/panel
  /widgets/table: /console/widgets/table

  # Migration from old blog structure
  /blog/release-notes: /blog
  /blog/news: /blog

  # External redirects
  /old-docs: https://archive.example.com
```

</Step>

<Step stepNumber="3">

### Build Your Site

Run your build command:

```bash
dotnet run -- build
```

MyLittleContentEngine generates an HTML file for each redirect mapping. Source path `/old-page` creates `old-page.html` in your output directory.

</Step>
</Steps>

> [!WARNING]
> Invalid YAML syntax will cause all redirects to fail silently. Validate your YAML before deploying.

## How Redirects Work

Both methods generate HTML pages that use meta refresh for instant redirection. The generated pages include:
- Automatic redirect with 0-second delay
- Fallback "click here" link for accessibility
- `noindex` tag to prevent search engine indexing

Search engines treat these redirects similarly to 301 redirects, making them suitable for static hosting platforms like GitHub Pages and Netlify where server-level redirects aren't available.

## Verify Redirects Work

<Steps>
<Step stepNumber="1">

### Build Your Site

Generate the static files including redirect HTML:

```bash
dotnet run -- build
```

For subdirectory deployments:

```bash
dotnet run -- build "/my-app/"
```

</Step>

<Step stepNumber="2">

### Install dotnet-serve

If you haven't already, install the [dotnet-serve](https://github.com/natemcmaster/dotnet-serve) tool:

```bash
dotnet tool install --global dotnet-serve
```

This is a one-time installation.

</Step>

<Step stepNumber="3">

### Serve the Build Output

Serve the generated static files locally:

```bash
dotnet serve -d output --default-extensions:.html
```

</Step>

<Step stepNumber="4">

### Test the Redirect

Navigate to the old URL in your browser. You should be redirected automatically to the new location.

</Step>
</Steps>

> [!NOTE]
> Redirects are only generated during the static build process (`dotnet run -- build`), not during development mode (`dotnet watch`). During `dotnet watch`, you're testing the Blazor SSR application, not the static HTML output.

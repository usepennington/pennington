---
title: "Implementing Redirects"
description: "Set up redirects using front matter properties or configuration files when content moves"
uid: "penn.guides.implementing-redirects"
order: 2050
---

Content moves. URLs shouldn't break. Penn supports redirecting old URLs to new locations when you reorganize, rename, or acknowledge that your original information architecture was a mistake. It happens. Penn doesn't judge.

## Choose Your Approach

**Front Matter Redirects** -- Add `redirect_url` to a markdown file's YAML front matter. Your front matter type must implement `IRedirectable`. Use this when:
- Moving or renaming individual pages
- Deprecating specific content
- Keeping the original file in your content structure as a breadcrumb

**Configuration File Redirects** -- Create a `_redirects.yml` file to manage multiple redirects. Use this when:
- Bulk migrations from another platform
- Redirecting URLs that don't have corresponding markdown files
- Managing redirects centrally in one place

You can combine both approaches. Penn aggregates them without complaint.

## Redirect a Single Page with Front Matter

### Step 1: Implement IRedirectable

Your front matter type needs to implement `IRedirectable` from `Penn.FrontMatter.Capabilities`:

```csharp
using Penn.FrontMatter;

public record MyFrontMatter : IFrontMatter, IRedirectable, ICrossReferenceable
{
    public string Title { get; init; } = "";
    public string? RedirectUrl { get; init; }
    public string? Uid { get; init; }
}
```

The built-in `DocFrontMatter` does not implement `IRedirectable` by default -- it's a deliberate omission, because documentation pages shouldn't casually redirect. If you need it, create a front matter type that includes the capability.

### Step 2: Add redirect_url to Your Markdown File

Open the markdown file you want to redirect and add `redirect_url` to its YAML front matter:

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

### Step 3: Build Your Site

Run your build command as usual:

```bash
dotnet run -- build
```

When the pipeline encounters a `DiscoveredItem` whose front matter implements `IRedirectable` with a non-null `RedirectUrl`, Penn generates a `RedirectSource` in the content source union. The output is an HTML redirect file at the original location instead of rendered markdown. The page is excluded from the table of contents.

> [!NOTE]
> Any markdown content in the file is ignored when `redirect_url` is set. The file exists solely as a redirect marker. Write a haiku in the body if you like. Nobody will see it.

## Redirect Multiple Pages with Configuration File

### Step 1: Create _redirects.yml in Content Root

Create a file named `_redirects.yml` in your content root directory (the path configured in `PennOptions.ContentRootPath`):

```yaml
redirects:
  /old-page: /new-page
  /archived: https://archive.example.com
  /docs/v1/guide: /docs/v2/guide
```

Each line maps a source path to a destination URL.

### Step 2: Organize with Comments

Group related redirects and document why they exist, because future-you will not remember:

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

### Step 3: Build Your Site

```bash
dotnet run -- build
```

Penn generates an HTML file for each redirect mapping. Source path `/old-page` creates `old-page/index.html` in your output directory.

> [!WARNING]
> Invalid YAML syntax causes all redirects in the file to fail silently. Validate your YAML before deploying. Penn trusts you to write valid YAML. Penn's trust is, historically, misplaced.

## How Redirects Work

Both approaches generate HTML pages with meta refresh for instant redirection. The generated pages include:

- Automatic redirect with 0-second delay
- Fallback "click here" link for accessibility
- `noindex` tag to prevent search engine indexing

The generated redirect HTML is minimal:

```html
<!DOCTYPE html>
<html>
<head>
  <meta http-equiv="refresh" content="0;url=/new-location">
</head>
</html>
```

Search engines treat meta refresh redirects similarly to 301 redirects, making them suitable for static hosting platforms like GitHub Pages and Netlify where server-level redirects aren't available. This is not ideal. It is sufficient.

## Verify Redirects Work

### Step 1: Build Your Site

Generate the static files:

```bash
dotnet run -- build
```

For subdirectory deployments:

```bash
dotnet run -- build "/my-app/"
```

### Step 2: Serve the Build Output

Install [dotnet-serve](https://github.com/natemcmaster/dotnet-serve) if you haven't:

```bash
dotnet tool install --global dotnet-serve
```

Serve the generated static files:

```bash
dotnet serve -d output --default-extensions:.html
```

### Step 3: Test the Redirect

Navigate to the old URL in your browser. You should be redirected automatically to the new location. If you aren't, check the generated HTML file for the source path and verify the destination URL is correct.

> [!NOTE]
> Redirects are generated during the static build process (`dotnet run -- build`), not during development mode (`dotnet watch`). During `dotnet watch`, you're testing the Blazor SSR application, where redirects are handled as HTTP 301 responses by the pipeline. The meta-refresh HTML is only for the static output.

## Redirects in the Pipeline

For the technically curious: when Penn encounters a redirect, it creates a `DiscoveredItem` with a `RedirectSource(UrlPath TargetUrl)` as the content source. The `ContentSource` union ensures the pipeline handles this case explicitly:

```csharp
public union ContentSource(
    MarkdownFileSource,
    RazorPageSource,
    RedirectSource,      // <- this one
    ProgrammaticSource
);
```

During static generation, the `OutputGenerationService` detects 301 responses and writes redirect HTML. During development, the server returns a proper HTTP redirect. Same intent, different mechanisms, both correct. Penn is nothing if not thorough about the boring parts.

---
title: "Implementing Redirects"
description: "Set up redirects using front matter properties when content moves"
uid: "penn.guides.implementing-redirects"
order: 2050
---

Content moves. URLs should not break when it does. Penn supports redirecting old URLs to new locations so that bookmarks, search engine indexes, and external links continue to work after you reorganize or rename pages.

## When to Use Redirects

Add a redirect when:

- You rename or move a page to a different path
- You consolidate multiple pages into one
- You deprecate content and want visitors sent to a replacement
- External sites link to a URL you no longer serve
- You restructure your content hierarchy and old paths become invalid

The redirect keeps the old URL alive. Instead of rendering the page's markdown content, Penn generates a small HTML file that sends the browser to the new location. The original markdown file stays in your content directory as a redirect marker, preserving the URL mapping in your source control history.

## The IRedirectable Interface

Penn's redirect support is built on a single capability interface in `Penn.FrontMatter`:

```csharp
public interface IRedirectable { string? RedirectUrl { get; } }
```

Any front matter type that implements `IRedirectable` gains the ability to redirect. When `RedirectUrl` is non-null, Penn treats the page as a redirect rather than rendering its content.

The `DocSiteFrontMatter` type provided by `Penn.DocSite` implements `IRedirectable` out of the box:

```csharp
public record DocSiteFrontMatter : IFrontMatter, IDraftable, ITaggable,
    ISectionable, ICrossReferenceable, IOrderable, IDescribable, IRedirectable
{
    public string Title { get; init; } = "";
    public string? RedirectUrl { get; init; }
    // ... other properties
}
```

If you use a custom front matter type, add `IRedirectable` to its interface list and include the `RedirectUrl` property:

```csharp
using Penn.FrontMatter;

public record MyFrontMatter : IFrontMatter, IRedirectable
{
    public string Title { get; init; } = "";
    public string? RedirectUrl { get; init; }
}
```

## Redirecting a Page

### Step 1: Add redirectUrl to the Front Matter

Open the markdown file at the old URL and add `redirectUrl` to its YAML front matter. The property name uses camelCase to match the C# property naming convention used by YamlDotNet deserialization:

```yaml
---
title: "Old Page Title"
redirectUrl: /guides/new-page-name/
---

This content will not be rendered. The page redirects to the new location.
```

The `redirectUrl` value can be:

- **An absolute path** on the same site: `/guides/new-page-name/`
- **An external URL** on a different domain: `https://other-site.com/replacement`

Use trailing slashes on absolute paths to match Penn's canonical URL format. External URLs redirect visitors off your site entirely, which is useful when content has moved to a different domain or when you are sunsetting a section in favor of a third-party resource.

### Step 2: Build Your Site

Run the static build:

```bash
dotnet run -- build
```

Penn detects the `redirectUrl` value during generation and writes a redirect HTML file at the original page's output path instead of rendering the markdown body.

Any markdown content in the file body is ignored when `redirectUrl` is set. The file exists solely as a redirect marker.

## How Redirects Flow Through the Pipeline

Understanding the pipeline helps when debugging unexpected behavior.

### Content Discovery

The `MarkdownContentService` discovers all markdown files, including those with `redirectUrl` set. Each file produces a `DiscoveredItem` with a route and a `ContentSource`. For redirect pages, the content source is a `RedirectSource`:

```csharp
public record RedirectSource(UrlPath TargetUrl);
```

`RedirectSource` is one of four cases in the `ContentSource` union:

```csharp
public union ContentSource(
    MarkdownFileSource,
    RazorPageSource,
    RedirectSource,
    ProgrammaticSource
);
```

The route for a redirect page is created by `ContentRouteFactory.ForRedirect()`, which builds a `ContentRoute` with the original page's canonical path and output file location.

### Static Generation

During `dotnet run -- build`, the `OutputGenerationService` crawls every discovered page by making HTTP requests to the running application. It creates an `HttpClient` with `AllowAutoRedirect = false` so it can detect redirect responses:

```csharp
using var client = new HttpClient(
    new HttpClientHandler { AllowAutoRedirect = false });
```

When the service receives a 301 (Moved Permanently) or 302 (Found) response, it writes a redirect HTML file to the output directory at the original page's path:

```html
<!DOCTYPE html>
<html><head>
<meta http-equiv="refresh" content="0;url=/guides/new-page-name/">
<link rel="canonical" href="/guides/new-page-name/">
</head></html>
```

The generated HTML contains two elements:

- A `<meta http-equiv="refresh">` tag that triggers an immediate browser redirect (the `0` means zero-second delay)
- A `<link rel="canonical">` tag that tells search engines the authoritative URL for this content

## Search Engine Behavior

Search engines treat meta-refresh redirects with a zero-second delay similarly to HTTP 301 redirects. Over time, they transfer ranking signals from the old URL to the new one and update their indexes to point to the destination.

This approach works on any static hosting platform, including GitHub Pages, Netlify, and S3, where you cannot configure server-level redirect rules. It is not a perfect substitute for a server-side 301, but it is effective for static site deployments.

The `<link rel="canonical">` tag reinforces the signal. Search engines use the canonical link as a strong hint about which URL should appear in search results, even before they follow the meta-refresh directive. Together, the two tags provide reliable redirect behavior for search crawlers and browsers alike.

## Verifying Redirects

### Step 1: Build the Site

Generate the static output:

```bash
dotnet run -- build
```

### Step 2: Serve the Output

Use any static file server to serve the generated files. With [dotnet-serve](https://github.com/natemcmaster/dotnet-serve):

```bash
dotnet tool install --global dotnet-serve
dotnet serve -d output --default-extensions:.html
```

### Step 3: Test the Redirect

Open the old URL in your browser. You should be redirected to the new location immediately.

If the redirect does not work, check these common issues:

1. Open the generated HTML file in the output directory at the old page's path and verify it contains the meta-refresh tag
2. Confirm the `url=` value in the meta-refresh tag points to the correct destination
3. Check the `redirectUrl` value in the markdown front matter for typos or incorrect paths
4. Verify that the destination page exists and returns a 200 response

For subdirectory deployments, remember that the redirect target URL is written as-is into the meta-refresh tag. If your site is hosted at `/my-app/`, use an absolute path that includes the subdirectory prefix, or use an external URL. See [Deploying to Subdirectories](xref:penn.guides.deploying-to-subdirectories) for details on base URL handling.

> [!NOTE]
> During development with `dotnet watch`, redirects are handled as HTTP 301 responses by the application pipeline. The meta-refresh HTML files are only generated during the static build. Both mechanisms send visitors to the same destination.

## Next Steps

- [Front Matter Properties](xref:penn.reference.front-matter-properties) -- full reference for all front matter properties including `redirectUrl`
- [Deploying to Subdirectories](xref:penn.guides.deploying-to-subdirectories) -- how base URL rewriting interacts with redirect target URLs

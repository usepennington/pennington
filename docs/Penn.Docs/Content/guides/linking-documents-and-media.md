---
title: "Link Types and Syntax"
description: "Reference for relative, absolute, cross-reference, and external links in Penn"
uid: "penn.guides.linking-documents-and-media"
order: 2010
---

A documentation site that works at `http://localhost:5000/` during development needs to keep working when deployed to `https://example.github.io/my-repo/`. Penn handles this through response-time URL rewriting, cross-reference resolution, and static file serving so that you write links once and deploy anywhere.

## The Deployment Challenge

The same set of Markdown files must produce correct links in all of these contexts:

- **Local development**: `http://localhost:5000/guides/search`
- **Production at root**: `https://docs.example.com/guides/search`
- **Subdirectory deployment**: `https://example.github.io/my-repo/guides/search`
- **Versioned subdirectory**: `https://example.github.io/my-repo/v4/guides/search`

Hardcoding the deployment path into your content creates a maintenance burden and forces every link to change when the hosting arrangement changes. Penn avoids this by letting you write canonical, root-relative links and rewriting them at response time based on the configured base URL.

This article covers the four link types Penn supports, how each one is processed, and when to use which.

## Relative Links

Relative links use `./` and `../` prefixes to navigate from the current document's location. Penn does not rewrite these. The browser resolves them based on the current page URL, which already includes the base path.

```markdown
<!-- Link to a sibling document in the same directory -->
[Search Guide](./implement-search-functionality)

<!-- Link to an image stored alongside the document -->
![Diagram](./images/architecture.png)

<!-- Link up to a parent directory -->
[All Guides](../guides)

<!-- Link to a child document -->
[Details](./advanced/configuration-details)
```

Relative links are best for tightly coupled content that lives in the same directory or an adjacent one. If you reorganize your content directories, relative links between moved files break.

## Absolute Links

Absolute links start with `/` and reference a path from the site root. Penn rewrites these automatically by prepending the configured base URL.

```markdown
<!-- Site-wide navigation -->
[Home](/)
[API Reference](/reference/api-overview)
[Getting Started](/getting-started/creating-first-site)

<!-- Static assets -->
![Logo](/images/logo.png)
[Download PDF](/files/whitepaper.pdf)
```

When deployed at `/my-repo/`, the link `/reference/api-overview` becomes `/my-repo/reference/api-overview` in the rendered HTML. During local development with no base URL configured, absolute links pass through unchanged.

Use absolute links for site-wide navigation and asset references. They survive content reorganization better than relative links because they reference the canonical path, and Penn's `BaseUrlRewritingProcessor` adjusts them for the deployment context.

## Cross-References

Cross-references link to content by its `uid` front matter property rather than by file path. This is the most resilient linking strategy. If you rename a file or move it to a different directory, the cross-reference still resolves as long as the target document keeps the same `uid`.

### Markdown Link Syntax

Use `xref:` as the link target in a standard Markdown link:

```markdown
[Deploying to Subdirectories](xref:penn.guides.deploying-to-subdirectories)
[Front Matter Properties](xref:penn.reference.front-matter-properties)
```

You provide the link text yourself. The `xref:uid` value resolves to the target document's canonical URL at render time.

### Inline Tag Syntax

Use the `<xref:uid>` tag for a self-labeling link:

```markdown
See <xref:penn.guides.deploying-to-subdirectories> for deployment details.
Check <xref:penn.reference.front-matter-properties> for the full list.
```

The `<xref:uid>` tag renders as an `<a>` element with the target document's `title` front matter value as the link text. Both syntaxes resolve to the same output; the tag form saves you from typing the link text when the document title is sufficient.

### How Cross-References Resolve

Cross-reference resolution happens in two phases during response processing.

**Phase 1: Tag resolution.** The `XrefResolvingService` scans the HTML for `<xref:uid>` patterns using a regex. For each match, it calls `XrefResolver.ResolveAsync(uid)` to look up the UID. If found, the tag is replaced with an `<a>` element pointing to the target's `CanonicalPath` with the target's `Title` as link text.

**Phase 2: Link resolution.** The service then parses the HTML with AngleSharp and finds all `<a>` elements whose `href` starts with `xref:`. For each one, it resolves the UID and replaces the `href` with the target URL. If the link text itself starts with `xref:`, it is also replaced with the target's title.

The `XrefResolver` builds a case-insensitive lookup dictionary from all registered `IContentService` instances. Each content service implements `GetCrossReferencesAsync()`, which returns a list of `CrossReference` records. The `MarkdownContentService` builds this list by scanning all content items whose front matter implements `ICrossReferenceable` and has a non-empty `Uid` property.

### Unresolved Cross-References

If a UID cannot be found, the link renders with `data-xref-error` and `data-xref-uid` attributes:

```html
<a href="xref:nonexistent.uid"
   data-xref-error="Reference not found"
   data-xref-uid="nonexistent.uid">nonexistent.uid</a>
```

Penn also emits a diagnostic warning. During development, check the `X-Penn-Diagnostic` response header for unresolved references:

```
X-Penn-Diagnostic: Warning|Unresolved xref: nonexistent.uid|XrefResolver
```

> [!TIP]
> Cross-reference resolution runs before base URL rewriting in the response processor pipeline (`XrefResolvingProcessor` has `Order = -10`, `BaseUrlRewritingProcessor` has `Order = 0`). This means resolved cross-references produce root-relative URLs that are then rewritten for the deployment context.

## External Links

Links that start with `https://` or `http://` are external. Penn does not modify them.

```markdown
[Markdig on GitHub](https://github.com/xoofx/markdig)
[AngleSharp Documentation](https://anglesharp.github.io/)
```

Protocol-relative URLs starting with `//` are also left alone.

## How BaseUrl Rewriting Works

The `BaseUrlRewritingProcessor` is an `IResponseProcessor` that rewrites root-relative URLs in HTML responses. Understanding its mechanics helps when debugging link issues.

### The Response Processor Pipeline

Penn's `ResponseProcessingMiddleware` captures the response body, filters the registered processors through `ShouldProcess`, sorts them by `Order`, and chains their `ProcessAsync` calls. The processors run in this order:

1. `XrefResolvingProcessor` (Order -10) -- resolves `xref:` links to canonical paths
2. `BaseUrlRewritingProcessor` (Order 0) -- prepends the base URL to root-relative paths

### What Gets Rewritten

The processor parses the HTML with AngleSharp and queries all elements that have `href`, `src`, or `action` attributes. For each attribute value that starts with `/` (but not `//`), it prepends the configured base URL:

```
Before: <a href="/guides/search">Search</a>
After:  <a href="/my-repo/guides/search">Search</a>

Before: <img src="/images/logo.png" />
After:  <img src="/my-repo/images/logo.png" />

Before: <form action="/api/submit">
After:  <form action="/my-repo/api/submit">
```

### The data-base-url Attribute

The processor also sets a `data-base-url` attribute on the `<body>` element:

```html
<body data-base-url="/my-repo">
```

Client-side JavaScript (such as SPA navigation scripts) reads this attribute to construct correct URLs without hardcoding the base path. If you write custom client-side code that builds URLs dynamically, read this attribute instead of assuming a root deployment.

### When Rewriting Is Skipped

The processor skips processing entirely when:

- The base URL is empty or `"/"` (root deployment -- no rewriting needed)
- The response status code is outside the 2xx range
- The content type is not `text/html` or `application/json`

Root deployments incur zero overhead from this processor.

## Static Files in Content Directories

Images, PDFs, and other non-Markdown files placed in your content directories are served automatically. Penn's `UsePenn()` middleware registers `StaticFileOptions` for each content source, mapping files from the content directory to the configured `BasePageUrl`.

Place an image next to your Markdown file:

```
Content/
  guides/
    linking-documents-and-media.md
    images/
      link-resolution-flow.png
```

Reference it with an absolute path:

```markdown
![Link resolution flow](/guides/images/link-resolution-flow.png)
```

Or with a relative path:

```markdown
![Link resolution flow](./images/link-resolution-flow.png)
```

The absolute version gets base URL rewriting. The relative version resolves through the browser. Both work.

During static site generation, `MarkdownContentService.GetContentToCopyAsync()` collects all non-content files (everything except `.md`, `.mdx`, `.razor`, `.yml`, and `.yaml`) from content directories and copies them to the output. Your images and PDFs end up in the right place without additional configuration.

## Testing Links

### During Development

Run the site with `dotnet watch` and verify links by clicking through the site. Check the browser's developer tools Network tab for 404 responses. Penn writes diagnostic information to the `X-Penn-Diagnostic` response header, which surfaces unresolved cross-references and other warnings.

### With a Test Base URL

Build the static site with a non-root base URL to verify that rewriting works:

```bash
dotnet run -- build "/test-base/"
```

Then serve the output directory with any static file server and verify that all links, images, and navigation resolve correctly:

```bash
npx serve output
```

Inspect the generated HTML to confirm that `<body data-base-url="/test-base">` is present and that all root-relative URLs have been rewritten.

### Checking Diagnostic Headers

During development, Penn adds `X-Penn-Diagnostic` headers to responses when issues are detected. Use `curl` or browser developer tools to inspect them:

```bash
curl -s -D - http://localhost:5000/guides/search | grep X-Penn-Diagnostic
```

Each diagnostic entry has the format `Severity|Message|Source`. A warning from the xref resolver looks like:

```
X-Penn-Diagnostic: Warning|Unresolved xref: some.missing.uid|XrefResolver
```

## Summary of Link Types

| Link Type | Example | Rewritten? | Best For |
|---|---|---|---|
| Relative | `./sibling` | No | Closely related content |
| Absolute | `/guides/search` | Yes | Site-wide navigation, assets |
| Cross-reference | `xref:uid` | Resolved, then rewritten | Internal content links |
| External | `https://example.com` | No | Third-party resources |

Use cross-references for links between content pages. Use absolute links for static assets. Use relative links sparingly for tightly coupled content that you expect to move together. See [Deploying to Subdirectories](xref:penn.guides.deploying-to-subdirectories) for the full subdirectory deployment workflow and [Front Matter Properties](xref:penn.reference.front-matter-properties) for configuring the `uid` property that powers cross-references.

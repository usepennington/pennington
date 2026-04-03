---
title: "Link Types and Syntax"
description: "Reference for relative, absolute, cross-reference, and external links in MyLittleContentEngine"
uid: "docs.guides.linking-documents-and-media"
order: 2010
---

The most challenging aspect of building static sites is creating links that work consistently across different
deployment scenarios. MyLittleContentEngine solves this through automatic BaseUrl-aware link rewriting that ensures
your links work seamlessly whether deployed at the root domain or in a subdirectory.

## The Deployment Challenge

Static sites often need to work in multiple deployment contexts:

- **Local development**: `http://localhost:5000/`
- **Production at root**: `https://mydomain.com/`
- **Production in subdirectory**: `https://mydomain.github.io/my-repo/`
- **Versioned in subdirectory**: `https://mydomain.github.io/my-repo/v4/`

The same site must generate correct links regardless of where it's deployed. MyLittleContentEngine's automatic link
rewriting handles this complexity for you.

## Understanding BaseUrl

The `BaseUrl` setting tells MyLittleContentEngine where your site will be deployed. This is critical for ensuring links work across different deployment scenarios and is now configured via command line arguments using OutputOptions.

### Quick BaseUrl Setup

```csharp
builder.Services.AddContentEngineService(_ => new ContentEngineOptions
{
    SiteTitle = "My Documentation Site",
    SiteDescription = "Technical documentation",
    ContentRootPath = "Content",
});
```

> [!TIP]
> BaseUrl is configured via command-line arguments at build time, not in `Program.cs`. See
> [Deploying to Subdirectories](xref:docs.guides.deploying-to-subdirectories) for the full setup.

## Link Types and Best Practices

MyLittleContentEngine supports different linking patterns to suit various use cases:

### Relative Links
Best for linking between closely related content:

```markdown
<!-- Within same directory -->
[Related Article](./related-article)
[Local Image](./images/diagram.png)

<!-- Parent/child navigation -->
[Parent Section](../index)
[Child Page](./subsection/details)
```

### Absolute Links
Best for site-wide navigation and assets:

```markdown
<!-- Site navigation -->
[Documentation](/docs)
[API Reference](/api)
[Home Page](/)

<!-- Static assets -->
![Site Logo](/images/logo.png)
[Download PDF](/files/guide.pdf)
```

### Cross-References (xref)
For linking to documented APIs and types:

```markdown
<!-- Link to API documentation -->
[ContentService](xref:MyLittleContentEngine.ContentService)
[Configuration Guide](xref:docs.guides.configuration)

<!-- Shorthand syntax using xref tags -->
<xref:MyLittleContentEngine.ContentService>
<xref:docs.guides.configuration>
```

The `<xref:uid>` syntax provides a convenient shorthand that automatically uses the target's title as the link text. Both approaches resolve to the same output, but the tag syntax is more concise when you want to use the document's actual title.

> [!NOTE]
> Cross-reference syntax works in both Markdown files and Razor pages. The processing happens during HTML generation, so you can use these patterns anywhere in your content.

### External Links
For referencing external resources:

```markdown
<!-- External sites -->
[Microsoft Docs](https://docs.microsoft.com)
[GitHub Repository](https://github.com/example/repo)
```

## Automatic Link Processing

MyLittleContentEngine automatically processes links in your content to ensure they work correctly across different deployment scenarios. All root-relative URLs (starting with `/`) are automatically adjusted based on your site's configured BaseUrl.

## Static Files in `ContentRootPath`

Static files like images, CSS, and JavaScript are served from the `ContentRootPath` directory and automatically
processed:

### In Markdown

```markdown
![Logo](/images/logo.png)
[Download PDF](/documents/guide.pdf)
```

### In Razor Components

```razor
<img src="/images/logo.png" alt="Logo" />
<a href="/documents/guide.pdf">Download Guide</a>
```

## Testing Your Links

During development, verify that your links work correctly:

1. **Run locally**: Use `dotnet watch` to test in development
2. **Check all link types**: Verify relative, absolute, and cross-reference links work
3. **Test static generation**: Use `dotnet run -- build "/"` to test static output locally

## Best Practices

1. **Choose the right link type**:
   - Use **relative links** for closely related content
   - Use **absolute links** for site-wide navigation and assets
   - Use **cross-references** for API documentation
   - Use **external links** for outside resources

2. **Be consistent**: Stick to a consistent linking pattern throughout your site

3. **Use descriptive link text**: Make links meaningful and accessible

4. **Test regularly**: Verify links work during development and after content updates

5. **Leverage automatic processing**: Let MyLittleContentEngine handle URL rewriting automatically


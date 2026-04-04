---
title: "Link Types and Syntax"
description: "Reference for relative, absolute, cross-reference, and external links in Penn"
uid: "penn.guides.linking-documents-and-media"
order: 2010
---

The most challenging aspect of building static sites is creating links that work consistently across different deployment scenarios. Penn solves this through automatic BaseUrl-aware link rewriting that ensures your links work whether deployed at a root domain or buried three subdirectories deep in a GitHub Pages site. You are welcome to attempt this yourself. Penn will wait.

## The Deployment Challenge

Static sites often need to work in multiple deployment contexts:

- **Local development**: `http://localhost:5000/`
- **Production at root**: `https://mydomain.com/`
- **Production in subdirectory**: `https://mydomain.github.io/my-repo/`
- **Versioned in subdirectory**: `https://mydomain.github.io/my-repo/v4/`

The same site must generate correct links regardless of where it ends up. Penn's `ResponseProcessingMiddleware` rewrites root-relative URLs at render time using the configured `OutputOptions`, so you don't have to think about it.

## Understanding BaseUrl

The `CanonicalBaseUrl` setting in <xref:T:Penn.Infrastructure.PennOptions> tells Penn where your site will be deployed:

```csharp
builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Documentation Site";
    penn.SiteDescription = "Technical documentation";
    penn.CanonicalBaseUrl = "https://mydomain.com";
    penn.ContentRootPath = "Content";
});
```

> [!TIP]
> For subdirectory deployments, the base URL is configured via command-line arguments at build time, not in `Program.cs`. Run `dotnet run -- build "/my-repo/"` to set the base path for static generation.

## Link Types and Best Practices

Penn supports different linking patterns. Each has its place. None is perfect. Such is life.

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

Best for site-wide navigation and assets. Penn rewrites these automatically based on the configured base URL:

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

For linking to content by UID rather than path. This is the civilized option. If your front matter type implements <xref:T:Penn.FrontMatter.ICrossReferenceable>, Penn resolves `xref:` links to the correct URL via `MarkdownContentService.GetCrossReferencesAsync()`, which maps UIDs to <xref:T:Penn.Routing.ContentRoute> instances using `ContentRoute.CanonicalPath`:

```markdown
<!-- Link to content by UID -->
[Configuration Guide](xref:penn.guides.configuration)
[Content Service](xref:penn.api.content-service)

<!-- Shorthand syntax using xref tags -->
<xref:penn.guides.configuration>
<xref:penn.api.content-service>
```

The `<xref:uid>` syntax automatically uses the target document's `Title` as the link text. Both forms resolve to the same output.

> [!NOTE]
> Cross-reference resolution requires that the target content's front matter implements `ICrossReferenceable` and has a non-empty `Uid` property. Penn is not psychic. If the UID doesn't exist, the link renders as-is, which is embarrassing but not fatal.

### External Links

For resources that exist outside your site. Penn leaves these alone, because it knows its place:

```markdown
[Microsoft Docs](https://docs.microsoft.com)
[GitHub Repository](https://github.com/example/repo)
```

## Automatic Link Processing

Penn automatically processes all root-relative URLs (starting with `/`) in rendered content, adjusting them based on the configured base URL. This happens in the response processing middleware, after Markdown rendering. You write `/docs/guide` and Penn rewrites it to `/my-repo/docs/guide` if that's where you're deployed.

This means you never write deployment-aware links in your content. You write canonical paths. Penn handles the rest.

## Static Files in Content Directories

Static files like images, CSS, and JavaScript placed in your content directories are served automatically. Penn's `UsePenn()` middleware configures `StaticFileOptions` for each registered content source, mapping files from the content path to the configured `BasePageUrl`.

### In Markdown

```markdown
![Architecture Diagram](/images/architecture.png)
[Download the PDF](/documents/whitepaper.pdf)
```

### In Razor Components

```razor
<img src="/images/logo.png" alt="Logo" />
<a href="/documents/guide.pdf">Download Guide</a>
```

Both receive automatic base URL rewriting during response processing.

## Testing Your Links

During development, verify that your links work correctly:

1. **Run locally**: Use `dotnet watch` to test in development
2. **Check all link types**: Verify relative, absolute, and cross-reference links resolve
3. **Test static generation**: Use `dotnet run -- build "/"` to generate static output and inspect the HTML

## Best Practices

1. **Use cross-references for internal links**: They survive content reorganization. Relative links do not.
2. **Use absolute links for assets**: Let Penn rewrite them for deployment.
3. **Use descriptive link text**: "Click here" is not descriptive. Penn cannot fix your prose.
4. **Test with a base URL**: Run `dotnet run -- build "/test-base/"` to verify links work in subdirectory deployments.

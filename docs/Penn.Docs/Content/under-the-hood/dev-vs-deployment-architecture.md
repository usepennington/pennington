---
title: "Development vs Deployment Architecture"
description: "Understanding how MyLittleContentEngine works across development, build, and production phases"
uid: "docs.under-the-hood.dev-vs-deployment-architecture"
order: 3000
---

MyLittleContentEngine operates in three distinct phases, each with different architectural characteristics. Understanding these phases is essential for effectively developing and deploying your content sites.

## The Three Phases

MyLittleContentEngine is a **hybrid static site generator** that uses Blazor Server-Side Rendering (SSR) as its rendering engine. This unique approach provides the best of both worlds: a rich development experience with hot reload capabilities, and a fully static output for production deployment.

The three phases are:

1. **Development Mode** - Running your site locally with `dotnet run` or `dotnet watch`
2. **Build/Generation Phase** - Generating static files with `dotnet run -- build`
3. **Production Serving** - Hosting the generated static files

## Development Mode

When you run your application with `dotnet run` or `dotnet watch`, MyLittleContentEngine operates as a standard ASP.NET Core web application with Blazor SSR.

### How It Works

Your application starts a web server that handles requests dynamically. When a user (or you during development) requests a page:

1. The ASP.NET Core routing system matches the URL to a Blazor component
2. The component requests content from the appropriate `IContentService` implementation
3. The content service lazily loads and caches content in memory
4. The Blazor component renders the content as HTML
5. The response is sent to the browser

### Content Service Behavior

Content services like `MarkdownContentService` use lazy loading with caching:

- **On first request**: The service processes all content files (e.g., all markdown files) and builds an in-memory index
- **Subsequent requests**: Content is served from the cache, making responses very fast
- **On file changes**: The file watcher invalidates the cache, triggering a fresh reload on the next request

This caching strategy provides excellent performance while keeping content always up-to-date during development.

### Hot Reload

MyLittleContentEngine includes a sophisticated hot reload system:

- File watchers monitor your content directories for changes
- When a file changes (e.g., you save a markdown file), the system invalidates the cached content
- The next page request triggers a complete reprocessing of content
- Your browser sees the updated content immediately

This enables a smooth development workflow where you can edit content and see changes instantly.

### Static Assets

During development, static assets are served directly from their source locations:

- Files in `wwwroot/`
- Content directory assets (images, downloads, etc.)
- Razor Class Library assets (like `scripts.js` from MyLittleContentEngine.UI)

All assets are served dynamically through middleware, allowing you to modify them and see changes immediately.

## Build/Generation Phase

When you run `dotnet run -- build`, MyLittleContentEngine switches from dynamic serving to static site generation.

### How It Works

The generation process follows these steps:

1. **Temporary Server Start**: A web server starts temporarily (just like in development mode)
2. **Page Collection**: The `OutputGenerationService` asks all registered content services for their pages via `IContentService.GetPagesToGenerateAsync()`
3. **Output Preparation**: The output directory is cleared and recreated
4. **Asset Collection**: Static assets are collected from all sources via `IContentService.GetContentToCopyAsync()`
5. **Page Generation**: Each page is rendered by making HTTP requests to the temporary server
6. **Server Shutdown**: The temporary server stops, leaving only static files

### The Role of IContentService

During generation, `IContentService` implementations play a crucial role:

- **`GetPagesToGenerateAsync()`**: Returns a list of all pages to render
  - `MarkdownContentService` returns one page per markdown file, plus tag pages
  - `ApiReferenceContentService` returns pages for namespaces, types, etc.
  - Custom content services can contribute their own pages

- **`GetContentToCopyAsync()`**: Returns static assets to copy
  - Image files, downloads, or other resources that don't need rendering

- **`GetContentToCreateAsync()`**: Returns dynamically generated files
  - Sitemap.xml, RSS feeds, search indexes, etc.

### Priority-Based Generation

Pages are generated in priority order to handle dependencies:

- **MustBeFirst** (0): Pages that other pages depend on
- **Normal** (50): Standard content pages
- **MustBeLast** (100): Pages that depend on other generated content (like CSS files that scan HTML)

Within each priority level, pages are generated in parallel for optimal performance.

### The HTTP Trick

The generation process uses a clever technique: it makes HTTP requests to the running application, just as a browser would. This means:

- Pages are rendered using exactly the same code path as development
- No duplicate rendering logic is needed
- The generated HTML is exactly what users would see if you ran the site dynamically

## Production Serving

After generation completes, you have a folder full of static HTML files and assets that can be deployed anywhere.

### What Gets Generated

The output directory contains:

- **HTML files**: One file per page (e.g., `blog/my-post.html`)
- **Static assets**: JavaScript, CSS, images, downloads
- **Generated files**: Sitemap, RSS feed, search index
- **Directory structure**: Mirrors your URL structure

### How It Differs from Development

The generated HTML has been transformed:

1. **Cross-references resolved**: `<xref:some.uid>` links are converted to proper `<a>` tags
2. **Base URLs applied**: If you built with a base path (e.g., `/my-blog/`), all URLs are rewritten
3. **No server required**: Everything is baked into the HTML

### Hosting Requirements

The static output can be hosted on any static file server:

- GitHub Pages
- Netlify
- Azure Static Web Apps
- Any web server (nginx, Apache, IIS)

The only requirement is that the host can serve HTML files and handle routing (e.g., serving `/about.html` when `/about` is requested). Most modern static hosts handle this automatically.

## Complete Walkthrough Example

Let's follow a blog post through all three phases to see how everything connects.

### Phase 1: Development

You create a new markdown file:

```markdown
---
title: "My First Blog Post"
description: "Learning how MyLittleContentEngine works"
uid: "blog.first-post"
---

# My First Blog Post

This is my first post using MyLittleContentEngine!
```

**What happens:**

1. You save `Content/blog/my-first-post.md`
2. The file watcher detects the change and invalidates the `MarkdownContentService` cache
3. You navigate to `http://localhost:5000/blog/my-first-post` in your browser
4. The Blazor routing matches the URL to a content page component
5. The component calls `MarkdownContentService.GetRenderedContentPageByUrlOrDefault("/blog/my-first-post")`
6. Since the cache was invalidated, the service:
   - Scans all markdown files in the content directory
   - Finds your new file and processes it
   - Parses the front matter
   - Converts markdown to HTML
   - Stores the result in memory
7. The component receives the rendered HTML and displays it
8. The browser shows your blog post

**Subsequent visits** to this page (or any other page) use the cached content until you modify a file again.

### Phase 2: Build

You're ready to deploy, so you run:

```bash
dotnet run -- build "/" "output"
```

**What happens:**

1. The application starts a temporary web server on a random port
2. `OutputGenerationService` calls `MarkdownContentService.GetPagesToGenerateAsync()`
3. The markdown service processes all files (including your blog post) and returns:
   - A page entry for `/blog/my-first-post`
   - Pages for any other markdown files
   - Pages for tag listings
4. The service also calls `GetContentToCopyAsync()` to find images and other assets
5. For your blog post, the generator:
   - Makes an HTTP GET request to `http://localhost:[port]/blog/my-first-post`
   - The server renders it (same as development)
   - Saves the HTML to `output/blog/my-first-post.html`
6. All other pages are generated in parallel
7. Static assets are copied to the output directory
8. The temporary server shuts down

### Phase 3: Production

You deploy the `output/` folder to your hosting service.

**What happens:**

1. A user navigates to `https://yourdomain.com/blog/my-first-post`
2. The static file host serves `blog/my-first-post.html`
3. The browser receives pure HTML (no server-side processing)
4. Client-side JavaScript enhances the page with interactive features
5. The user sees your blog post

**Key differences from development:**

- No ASP.NET Core server running
- No content services executing
- No dynamic rendering
- Just static files being served by a basic web server

If you need to update the blog post, you edit the markdown file and rebuild. The development workflow stays the same; only the final deployment changes.

## Key Differences Summary

| Aspect | Development Mode | Build Phase | Production Serving |
|--------|-----------------|-------------|-------------------|
| **Runtime** | ASP.NET Core + Blazor SSR | Temporary server for generation | Static file host |
| **Content Processing** | Lazy load + cache, reprocess on file changes | All content processed once | N/A (pre-rendered) |
| **IContentService Role** | Serves content dynamically from cache | Provides lists of pages and assets to generate | Not present |
| **Hot Reload** | File watching invalidates cache | Not applicable | Not applicable |
| **Response Time** | Fast (from cache) after warmup | N/A | Instant (static files) |
| **URL Handling** | Runtime routing and middleware | Baked into generated HTML | Host serves static files |
| **Resource Usage** | Memory for caching content | Temporary (during generation only) | Minimal (just file serving) |
| **When to Use** | Local development and testing | Before deployment | End-user access |

## Understanding the Hybrid Approach

MyLittleContentEngine's architecture provides several advantages:

**For Developers:**
- Familiar ASP.NET development experience
- Instant feedback with hot reload
- Full debugging capabilities during development
- Content services work the same way in development and build

**For Users:**
- Fast page loads (static HTML)
- No server-side dependencies
- Works on any static hosting platform
- Excellent performance and reliability

**For Content:**
- Single source of truth (your markdown files, API metadata, etc.)
- Content services define how content is processed
- Same rendering logic in development and production
- Extensible through custom `IContentService` implementations

This hybrid approach makes MyLittleContentEngine unique among static site generators: you develop with the full power of a web framework, but deploy with the simplicity of static files.

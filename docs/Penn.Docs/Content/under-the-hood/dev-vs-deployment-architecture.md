---
title: "Development vs Deployment Architecture"
description: "How Penn runs as a live dev server and a static site generator from the same codebase"
uid: "penn.under-the-hood.dev-vs-deployment-architecture"
order: 3000
---

Penn is two things at once: a live ASP.NET Core web application during development and a static site generator at build time. Same codebase, same content pipeline, same rendering logic. The difference is one command-line argument.

This dual-mode design means you get hot reload, full debugging, and a real web server during development -- then flip a switch and produce a folder of static HTML files for deployment. No separate build tool, no template language mismatch, no "it looked different in dev."

## The Two Modes

### Development: `dotnet watch`

```bash
dotnet watch
```

Penn starts as a standard ASP.NET Core application with Blazor SSR. Every page request runs through the full content pipeline: content services discover files, the parser extracts front matter and markdown, the renderer produces HTML, and the Blazor component renders it into a page with layout, navigation, and all the trimmings.

Content is cached after the first request. Edit a markdown file, and the file watcher invalidates the cache. Refresh your browser. Done.

### Static Build: `dotnet run -- build`

```bash
dotnet run -- build
```

Penn starts the same ASP.NET Core application, but instead of waiting for browser requests, it drives itself. `RunOrBuildAsync` detects the `build` argument and switches to generation mode:

```csharp:xmldocid
T:Penn.Infrastructure.PennExtensions
```

The generation flow:

1. Start the web server (same as dev mode).
2. Get the `OutputGenerationService` from DI.
3. Call `GenerateAsync()` with the server's base address.
4. Shut down the server.
5. Exit.

The app starts, generates, and stops. No long-running process.

## RunOrBuildAsync: The Mode Switch

The mode decision happens in a single method:

```csharp
public static async Task RunOrBuildAsync(this WebApplication app, string[] args)
{
    StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);

    if (args.Length > 0 && args[0].Equals("build", StringComparison.OrdinalIgnoreCase))
    {
        await app.StartAsync();
        var generator = app.Services.GetRequiredService<OutputGenerationService>();
        var addresses = app.Urls.Any() ? app.Urls : ["http://localhost:5000"];
        await generator.GenerateAsync(addresses.First());
        await app.StopAsync();
    }
    else
    {
        await app.RunAsync();
    }
}
```

No `build` argument? `app.RunAsync()` -- standard dev server. Got `build`? Start, generate, stop. The rest of your `Program.cs` is identical for both modes.

## OutputGenerationService: The Self-Crawler

Static generation works by making HTTP requests to the running application. Yes, really. The app talks to itself.

`OutputGenerationService` does the following:

1. **Discover pages.** Iterates all `IContentService` implementations and calls `DiscoverAsync()` to collect every page URL and its output file path.

2. **Prepare the output directory.** Wipes the output folder (default `output/`) and creates it fresh.

3. **Copy static assets.** Each content service provides a list of files to copy (images, downloads, etc.) via `GetContentToCopyAsync()`.

4. **Fetch and save each page.** For each discovered page, makes an HTTP GET request to the running server. The response HTML is saved to the corresponding output file.

The HTTP self-crawl is the key trick. It means:

- Pages are rendered through exactly the same code path as development. No special "static rendering" mode.
- Layouts, components, middleware, response processors -- everything runs normally.
- The generated HTML is byte-for-byte what a browser would receive.

Pages are fetched in parallel using `Parallel.ForEachAsync` for performance. Redirect responses (301) produce a tiny HTML file with a `<meta http-equiv="refresh">` tag.

## OutputOptions: Controlling the Build

Build options are parsed from command-line arguments:

```bash
dotnet run -- build /my-base-url output-folder
```

- **Base URL** (optional, default `/`): Prepended to all URLs. Useful when deploying to a subdirectory like GitHub Pages (`/my-repo/`).
- **Output directory** (optional, default `output`): Where the static files land.

The `BaseUrlRewritingProcessor` middleware rewrites URLs in the generated HTML when a non-root base URL is configured. This means your markdown files use root-relative paths (`/docs/getting-started/`) and they get rewritten to `/my-repo/docs/getting-started/` at build time.

## What Gets Generated

After a build, the output directory contains:

```
output/
+-- index.html
+-- docs/
|   +-- getting-started/
|   |   +-- index.html
|   +-- configuration/
|       +-- index.html
+-- _spa-data/
|   +-- index.json
|   +-- docs/getting-started.json
+-- images/
|   +-- logo.png
+-- styles.css
+-- scripts.js
```

- **HTML files** for every discovered page.
- **SPA data files** (if SPA navigation is configured) as JSON envelopes.
- **Static assets** copied from content directories and wwwroot.
- **Generated files** like sitemaps, RSS feeds, and search indexes (from `GetContentToCreateAsync()`).

Clean URLs work because each page becomes `slug/index.html`. Static file hosts serve `index.html` automatically when a directory is requested.

## Development vs Build: What Changes

| Aspect | Development (`dotnet watch`) | Build (`dotnet run -- build`) |
|---|---|---|
| **Server** | Runs continuously | Starts, generates, stops |
| **Rendering** | On-demand per request | All pages, in parallel |
| **Caching** | File-watch invalidation | Not needed (one-shot) |
| **Drafts** | Visible (pipeline renders them) | Filtered at Generate stage |
| **Base URL rewriting** | Not applied | Applied to all output |
| **Output** | HTTP responses | Static HTML files |
| **File watching** | Active (hot reload) | Not meaningful |

### Draft Handling

During development, draft pages (`IsDraft: true` in front matter) are rendered normally -- you can preview them in your browser. During a static build, the Generate stage filters them out. They go through Discover, Parse, and Render (so you still get build errors if a draft has broken markdown), but they are not written to the output directory.

### Same Pipeline, Different Exit

The content pipeline (Discover, Parse, Render) runs identically in both modes. The only difference is what happens at the end:

- **Dev mode**: Rendered content is served via HTTP to your browser.
- **Build mode**: Rendered content is fetched via HTTP (from self) and saved to disk.

This is why Penn uses the "self-crawl" approach instead of rendering directly to files. The rendering code does not need to know which mode it is in. It just serves HTML, same as always.

## Hosting the Output

The generated static files work on any static file host:

- **GitHub Pages**: Push the output directory.
- **Netlify / Vercel**: Point at the output directory.
- **Azure Static Web Apps**: Deploy the output.
- **nginx / Apache / IIS**: Serve the directory. Configure clean URL support if your host does not handle `index.html` fallback automatically.
- **S3 + CloudFront**: Upload and configure index document support.

No server runtime needed. No .NET, no Node.js, no anything. Just files.

## The Complete Lifecycle

Here is the full journey of a markdown file through both modes:

**During development:**

```
You save getting-started.md
  --> FileWatcher detects change
  --> Content cache invalidated
  --> You refresh browser
  --> ASP.NET routes request to content page component
  --> Pipeline: Discover --> Parse --> Render
  --> Blazor SSR renders the full page
  --> Browser shows your changes
```

**During build:**

```
You run: dotnet run -- build
  --> ASP.NET starts on localhost
  --> OutputGenerationService discovers all pages
  --> Fetches /docs/getting-started/ from localhost
  --> Pipeline: Discover --> Parse --> Render (same as dev!)
  --> Blazor SSR renders the full page (same as dev!)
  --> HTML saved to output/docs/getting-started/index.html
  --> Server stops
  --> Deploy output/ to your host
```

Same pipeline. Same rendering. Same HTML. Different delivery mechanism. That is the entire trick.

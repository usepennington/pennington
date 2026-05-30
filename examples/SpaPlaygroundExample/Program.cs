using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Routing;

// SPA engine playground — a minimal Pennington site whose only purpose is to
// make spa-engine.js visible.
//
// Two elements are marked data-spa-region: the <header> and the <main> article.
// The engine intercepts in-site link clicks, fetches the destination as full
// HTML, parses it, and replaces those regions' innerHTML in place. Anything
// outside a marked region is never touched — the <nav> here keeps its DOM
// across every navigation, so the "Navigations observed" counter accumulates
// and any scroll position or focus you give it survives.
//
// An inline <script> in Layout() below subscribes to the engine's three
// lifecycle events (spa:before-navigate, spa:commit, spa:diagnostics) and
// writes them to console.log plus an on-page event log so you can watch the
// swaps land without DevTools open.

var builder = WebApplication.CreateBuilder(args);

// AddPennington wires the markdown pipeline. ContentRootPath points at the
// Content/ folder; AddMarkdownContent registers every .md file under that
// folder, parsed with DocFrontMatter as the front-matter shape.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "SPA Playground";
    penn.ContentRootPath = "Content";
    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

var app = builder.Build();

// Serve Razor class library static web assets — specifically spa-engine.js at
// /_content/Pennington.UI/spa-engine.js. MapStaticAssets is the .NET 9+
// manifest-aware endpoint that includes RCL assets. DocSite-based hosts get
// this for free via UseDocSite(); bare hosts have to opt in.
app.MapStaticAssets();

// UsePennington adds Pennington's middleware: response processing (which
// injects the live-reload script and the dev diagnostics overlay you see in
// the bottom-right), redirect handling, locale detection, and a few endpoints
// (sitemap.xml, search-index). It deliberately does not register a catch-all
// route — that's the MapGet below, so the host owns its routing shape.
app.UsePennington();

// Catch-all: ask IPageResolver for the first content route matching the request,
// then wrap the rendered HTML in Layout() below to add the chrome the SPA engine
// understands.
app.MapGet("/{*path}", async (string? path, IPageResolver resolver) =>
{
    var requested = new UrlPath(path ?? string.Empty).EnsureLeadingSlash();

    if (await resolver.ResolveAsync(requested) is not { } page)
    {
        return Results.NotFound();
    }

    return Results.Content(Layout(page.Metadata.Title, page.Content.Html), "text/html");
});

// Run live in dev mode, or crawl every discovered route and emit static HTML
// when invoked as `dotnet run -- build <baseUrl> <outputDir>`. Both modes go
// through the same MapGet above, so what you see live is what ends up on disk.
await app.RunOrBuildAsync(args);
return;

// Renders the page chrome around the article HTML. Header and main carry
// data-spa-region so the engine swaps them on navigation. The nav sits outside
// any region — its DOM persists across navigations, which is why the navigation
// counter and scroll position survive while the article body changes.
static string Layout(string title, string contentHtml) =>
    // lang=html
    $$"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1" />
      <title>{{title}} — SPA Playground</title>
      <style>
        body { font-family: ui-monospace, SFMono-Regular, Menlo, monospace; max-width: 56rem; margin: 0 auto; padding: 1rem; color: #222; }
        .tag { display: inline-block; padding: 2px 8px; font-size: 11px; font-weight: bold; color: white; margin-bottom: 8px; letter-spacing: 0.05em; }
        .tag.swap { background: #c00; }
        .tag.persist { background: #06c; }
        header { border: 4px solid #c00; padding: 1rem; margin-bottom: 1rem; }
        nav { border: 4px solid #06c; padding: 1rem; margin-bottom: 1rem; }
        main { border: 4px solid #c00; padding: 1rem; margin-bottom: 1rem; }
        nav ul { margin: 0; padding-left: 1.25rem; }
        nav .counter { display: inline-block; margin-left: 0.5rem; padding: 1px 6px; background: #eef; border: 1px solid #06c; font-size: 12px; }
        h1 { margin-top: 0; }
        #event-log { background: #111; color: #6f6; font-size: 12px; padding: 0.75rem; height: 14rem; overflow-y: auto; border: 2px dashed #444; white-space: pre-wrap; }
        #event-log .hdr { color: #aaa; }
        #event-log .evt { color: #fc6; }
      </style>
    </head>
    <body>
      <!-- The SPA engine queries elements by [data-spa-region] and replaces
           their innerHTML on each navigation. Two regions live in this layout:
           header and content. Their inner DOM is thrown away on each click. -->
      <header data-spa-region="header">
        <span class="tag swap">SWAP REGION · data-spa-region="header"</span>
        <h1>{{title}}</h1>
      </header>

      <!-- No data-spa-region. The engine never queries this element, so the
           same <nav> nodes survive every navigation along with whatever state
           lives inside (the counter below, scroll position, focus). This is
           how Pennington's docs sidebar keeps its scroll/expand state. -->
      <nav>
        <span class="tag persist">PERSISTENT · no data-spa-region</span>
        <strong>Pages</strong>
        <ul>
          <li><a href="/">Page one</a></li>
          <li><a href="/page-two/">Page two</a></li>
          <li><a href="/page-three/">Page three</a></li>
        </ul>
        <div>Navigations observed: <span class="counter" data-nav-count>0</span></div>
      </nav>

      <main data-spa-region="content">
        <span class="tag swap">SWAP REGION · data-spa-region="content"</span>
        <article>{{contentHtml}}</article>
      </main>

      <div>
        <strong>Event log</strong> <small>(also written to <code>console.log</code>)</small>
        <pre id="event-log"></pre>
      </div>

      <!-- The engine itself. Intercepts same-origin <a> clicks, fetches the
           destination URL as full HTML, parses it, swaps regions, merges head
           deltas, and dispatches three lifecycle events. -->
      <script src="/_content/Pennington.UI/spa-engine.js" defer></script>

      <!-- Inspector. Subscribes to the engine's events and writes them to
           console.log plus the on-page event log. This script sits in the
           layout (outside any region), so its event listeners outlive every
           navigation — listeners attached inside a swapped region would be
           thrown away with the DOM and have to be re-bound on spa:commit. -->
      <script>
        (function () {
            const out = document.getElementById('event-log');
            const counter = document.querySelector('[data-nav-count]');

            // The engine uses URL and Document objects in event detail; both
            // need a custom JSON replacer to render readably in the log.
            function fmt(detail) {
                return JSON.stringify(detail, function (k, v) {
                    if (v instanceof URL) return v.href;
                    if (typeof Document !== 'undefined' && v instanceof Document) return '[Document]';
                    return v;
                }, 2);
            }

            function write(label, detail) {
                console.log('[' + label + ']', detail);
                if (!out) return;
                const time = new Date().toISOString().slice(11, 23);
                const block = document.createElement('div');
                block.innerHTML = '<span class="hdr">' + time + '</span> '
                    + '<span class="evt">' + label + '</span>\n'
                    + fmt(detail);
                out.appendChild(block);
                out.scrollTop = out.scrollHeight;
            }

            // spa:before-navigate { url, slug } — link click intercepted, fetch about to start.
            document.addEventListener('spa:before-navigate', e => write('spa:before-navigate', e.detail));

            // spa:commit { url, slug, doc } — region swap done. doc is the parsed
            // destination Document; consumers can read it to patch persistent
            // chrome (active link state, breadcrumb, etc.) outside any region.
            document.addEventListener('spa:commit', e => {
                write('spa:commit', e.detail);
                if (counter) counter.textContent = String(parseInt(counter.textContent || '0', 10) + 1);
            });

            // spa:diagnostics [Diagnostic] — only fires when the response carries a
            // <script type="application/spa-diagnostics+json"> payload (dev only).
            document.addEventListener('spa:diagnostics', e => write('spa:diagnostics', e.detail));

            write('page-loaded', { url: location.href });
        })();
      </script>
    </body>
    </html>
    """;
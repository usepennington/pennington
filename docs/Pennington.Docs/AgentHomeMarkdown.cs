namespace Pennington.Docs;

/// <summary>
/// Machine-readable home page served at <c>/index.md</c>. The human landing page at <c>/</c>
/// (<c>Components/Index.razor</c>) is a marketing splash that converts poorly to markdown; this
/// is its purpose-built equivalent for agents that request <c>text/markdown</c> — a concise
/// orientation plus the map of how to read the rest of the site as markdown. Served via a
/// <c>MapGet</c> in <c>Program.cs</c> (so it never becomes a routed content page) and listed in
/// <c>/llms.txt</c> through <c>WithLlmsTxtEntry</c>.
/// </summary>
public static class AgentHomeMarkdown
{
    /// <summary>Display title for the <c>/llms.txt</c> entry.</summary>
    public const string Title = "Pennington documentation — home";

    /// <summary>Description rendered after the <c>/llms.txt</c> entry link.</summary>
    public const string Description =
        "Machine-readable entry point: what Pennington is, how to read the docs as markdown, and the documentation map.";

    /// <summary>The markdown body returned at <c>/index.md</c>.</summary>
    public const string Body =
        """
        # Pennington

        Pennington is a static content engine for .NET. You write Markdown with YAML front matter
        (and, when you want them, Razor components inside the prose); one ASP.NET host serves it
        live with hot reload while you write, and crawls its own routes to flat HTML when you
        build. No Node, no npm, no bundler — `dotnet run` in development, `dotnet run -- build` to
        emit the static site. Search, RSS, sitemap, structured data, llms.txt, locale routing, and
        an API reference generated from your assemblies' XML docs are built in.

        The page a human sees at `/` is a marketing landing page. This is its machine-readable
        equivalent: skip the rendered HTML and read the docs as Markdown instead.

        ## Read this site as Markdown

        Every content page is published as clean Markdown at its own URL with `index.md` appended —
        for example `/how-to/feeds/llms-txt/index.md`. Pages advertise that copy with a
        `<link rel="alternate" type="text/markdown">` tag, and an agent that sends
        `Accept: text/markdown` is steered to it.

        - `/llms.txt` — index of every page, grouped by section, with per-subtree splits and token
          estimates. Start here to plan what to fetch.
        - `/{any page path}/index.md` — that single page as front-matter-stamped Markdown (canonical
          URL, content hash, token estimate, then the body).

        ## Documentation map

        The docs follow Diátaxis — split by what you are trying to do. Fetch `/llms.txt` for the
        complete list; these are the entry points:

        - **Tutorials** (learn by building) — `/tutorials/getting-started/first-site/index.md`.
        - **How-to** (solve one task) — for example `/how-to/feeds/llms-txt/index.md`; the full set
          is grouped under "How-to" in `/llms.txt`.
        - **Reference** (look something up) — the API surface, front-matter keys, and CLI flags are
          their own subtree: `/reference/llms.txt`.
        - **Explanation** (understand the design) — `/explanation/core/mental-model/index.md`.

        ## Quickstart

        ```
        dotnet new web -n my-site
        cd my-site
        dotnet add package Pennington
        dotnet run
        ```

        Wire `AddPennington` / `UseDocSite` (or `AddDocSite` for the batteries-included template),
        point it at a `Content/` folder of Markdown, and run. The full walkthrough is
        `/tutorials/getting-started/first-site/index.md`.

        ## Porting an existing docs site

        If you are migrating from Docusaurus, VitePress, MkDocs, or Astro Starlight, read
        `/migrating-via-ai/index.md` first — it names the primitives to reuse and the dead ends to
        avoid.
        """;
}

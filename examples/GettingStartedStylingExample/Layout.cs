namespace GettingStartedStylingExample;

using Pennington.Navigation;

/// <summary>
/// Minimal HTML layout helper — the bare-host equivalent of a DocSite/Razor
/// layout component. It wraps every rendered markdown page in a consistent
/// shell of utility-class-styled elements. Because the HTML returned here is
/// what flows through the ASP.NET response pipeline,
/// <c>CssClassCollectorProcessor</c> sees every utility token on its way to
/// the browser and the generated stylesheet stays in sync.
/// </summary>
/// <remarks>
/// Teaching points:
/// <list type="bullet">
/// <item><description>The <c>&lt;link rel="stylesheet" href="/styles.css"&gt;</c>
///   tag points at the endpoint registered by <c>UseMonorailCss()</c>.</description></item>
/// <item><description>Utility classes on <c>&lt;body&gt;</c>,
///   <c>&lt;header&gt;</c>, <c>&lt;nav&gt;</c>, <c>&lt;article&gt;</c>, and
///   <c>&lt;footer&gt;</c> all feed the collector once the page is rendered.</description></item>
/// <item><description>Swap <c>PrimaryColorName</c> in <c>Program.cs</c> and
///   the color behind every <c>text-primary-*</c>/<c>bg-primary-*</c>
///   token changes on the next request.</description></item>
/// </list>
/// </remarks>
public static class Layout
{
    /// <summary>
    /// Render the shared page shell around a pre-rendered markdown body.
    /// </summary>
    /// <param name="title">Front-matter title used in <c>&lt;title&gt;</c> and the H1.</param>
    /// <param name="navTree">Nav entries produced by <see cref="NavigationBuilder"/>.</param>
    /// <param name="bodyHtml">The markdown pipeline's rendered HTML.</param>
    public static string Render(string title, IReadOnlyList<NavigationTreeItem> navTree, string bodyHtml)
    {
        var navHtml = string.Join(
            "",
            navTree.Select(item =>
                $"<li><a class=\"text-primary-700 hover:text-primary-900 font-medium\" href=\"{item.Route.CanonicalPath.Value}\">{item.Title}</a></li>"));

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>{title}</title>
              <link rel="stylesheet" href="/styles.css" />
            </head>
            <body class="bg-base-50 text-base-900 min-h-screen">
              <div class="max-w-3xl mx-auto px-6 py-10">
                <header class="mb-8 border-b border-base-200 pb-4">
                  <a class="text-lg font-bold text-primary-700" href="/">My Styled Pennington Site</a>
                  <nav class="mt-2">
                    <ul class="flex gap-4 text-sm">{navHtml}</ul>
                  </nav>
                </header>
                <article class="prose">
                  <h1 class="text-3xl font-bold text-base-900 mb-4">{title}</h1>
                  {bodyHtml}
                </article>
                <footer class="mt-12 pt-4 border-t border-base-200 text-xs text-base-500">
                  Styled with MonorailCSS.
                </footer>
              </div>
            </body>
            </html>
            """;
    }
}

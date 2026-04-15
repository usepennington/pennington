namespace DocSiteScaffoldExample;

using Pennington.DocSite;

/// <summary>
/// Stage 2 — swap `AddPennington` for `AddDocSite` and populate
/// <see cref="DocSiteOptions"/>. The DocSite template registers Pennington
/// core internally, wires the Razor component layout, Mdazor, MonorailCSS,
/// SPA navigation, and the content resolver — all in a single call.
/// The middleware (`UseDocSite`) and run/build entry point arrive in
/// <see cref="Stage3"/>. Tutorial prose extracts the body of
/// <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is never
/// instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>Register DocSite services with areas and site options.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Scaffold Docs",
            Description = "A minimal DocSite scaffold showing AddDocSite and area routing.",
            GitHubUrl = "https://github.com/usepennington/pennington",
            HeaderContent = """<a href="/">Scaffold Docs</a>""",
            FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

            Areas =
            [
                new ContentArea("Guides", "guides"),
                new ContentArea("Reference", "reference"),
            ],
        });

        var app = builder.Build();

        await app.RunAsync();
    }
}
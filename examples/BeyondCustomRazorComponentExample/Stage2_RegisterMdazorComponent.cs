namespace BeyondCustomRazorComponentExample;

using BeyondCustomRazorComponentExample.Components;
using Mdazor;
using Pennington.DocSite;

/// <summary>
/// Stage 2 — the host after <c>services.AddMdazorComponent&lt;PricingCard&gt;()</c>
/// has been wired. DocSite already calls <c>services.AddMdazor()</c> under the
/// hood and registers the built-in Pennington.UI components, so the only line
/// the tutorial reader has to add is the single <c>AddMdazorComponent&lt;T&gt;()</c>
/// registration for their own component. Tutorial prose extracts
/// <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is never
/// instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>DocSite host with a custom Mdazor component registered.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Beyond Custom Razor Component",
            Description = "Authoring a Razor component and rendering it inline from markdown.",
            GitHubUrl = "https://github.com/usepennington/pennington",
            HeaderContent = """<a href="/">Beyond Custom Razor Component</a>""",
            FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",
        });

        // The one new line vs stage 1: tell Mdazor about the PricingCard type.
        // AddMdazorComponent<T>() is an IServiceCollection extension in the
        // Mdazor namespace (from the Mdazor NuGet package, transitively
        // referenced through Pennington.DocSite). It returns the same
        // IServiceCollection so it chains with further registrations.
        builder.Services.AddMdazorComponent<PricingCard>();

        var app = builder.Build();

        app.UseDocSite();

        await app.RunDocSiteAsync(args);
    }
}

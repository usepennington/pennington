namespace SubPathDeployableExample;

using Pennington.DocSite;

/// <summary>
/// Host configuration for the deployment demo. The how-to pages in §2.4
/// fence this helper so their code samples stay in lockstep with the
/// running app.
/// </summary>
internal static class ServiceConfiguration
{
    /// <summary>
    /// Builds the DocSite options used by the deployment demo. Kept in a
    /// named helper (rather than inline in <c>Program.cs</c>) so the
    /// deployment how-tos can reference a stable xmldocid fence target.
    /// </summary>
    public static DocSiteOptions BuildDocSiteOptions() => new()
    {
        SiteTitle = "Deploy Docs",
        Description = "A minimal DocSite used to demonstrate static build, base URL, and the five deployment host recipes.",
        GitHubUrl = "https://github.com/usepennington/pennington",
        HeaderContent = """<a href="/">Deploy Docs</a>""",
        FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

        Areas =
        [
            new ContentArea("Guides", "guides"),
        ],
    };
}

namespace DocSiteChromeOverridesExample;

using System.Reflection;
using Pennington.DocSite;

/// <summary>
/// Static helpers that show how the <see cref="DocSiteOptions"/> slot
/// seams are wired. Each method body is a fence target for how-to
/// <c>/how-to/extensibility/override-docsite-components</c>.
/// <para>
/// The companion <c>Program.cs</c> in this project calls
/// <see cref="BuildDocSiteOptions"/> from its <c>AddDocSite</c> factory, so
/// the overrides actually render in a running site — the how-to's step 6
/// fences that file directly.
/// </para>
/// </summary>
public static class SiteChromeOverrides
{
    /// <summary>
    /// A populated <see cref="DocSiteOptions"/> that exercises all four
    /// slot seams the how-to covers: <see cref="DocSiteOptions.AdditionalHtmlHeadContent"/>,
    /// <see cref="DocSiteOptions.ExtraStyles"/>,
    /// <see cref="DocSiteOptions.HeaderContent"/> / <see cref="DocSiteOptions.FooterContent"/>
    /// (the string-or-fragment content slots), and
    /// <see cref="DocSiteOptions.AdditionalRoutingAssemblies"/>.
    /// </summary>
    public static DocSiteOptions BuildDocSiteOptions() => new()
    {
        SiteTitle = "DocSite Chrome Overrides",
        SiteDescription = "Running DocSite that exercises every override seam on DocSiteOptions.",
        HeaderContent = """<span class="chrome-header" data-chrome-overrides="docsite-header">Chrome Overrides</span>""",
        FooterContent = """<span class="chrome-footer" data-chrome-overrides="docsite-footer">(c) 2026 Pennington</span>""",
        AdditionalHtmlHeadContent = BuildHtmlHeadContent(),
        ExtraStyles = BuildExtraStyles(),
        AdditionalRoutingAssemblies = BuildAdditionalRoutingAssemblies(),
        Areas =
        [
            new ContentArea("Guides", "guides"),
        ],
    };

    /// <summary>
    /// The HTML string that ends up inside <c>&lt;head&gt;</c> on every
    /// page. Kept hand-authored so the how-to can show both the literal-string
    /// pattern and a component-rendered variation via
    /// <see cref="Components.ExtraHeadFragment"/>.
    /// </summary>
    public static string BuildHtmlHeadContent() => """
        <meta name="x-chrome-overrides-head" content="extra-head-fragment">
        <link rel="preconnect" href="https://example.com">
        """;

    /// <summary>
    /// Extra stylesheet rules injected into the generated
    /// <c>/styles.css</c> through <see cref="DocSiteOptions.ExtraStyles"/>.
    /// Use this for <c>@@font-face</c> declarations or any rule the
    /// MonorailCSS utility scan will not discover on its own.
    /// </summary>
    public static string BuildExtraStyles() => """
        .chrome-header { font-weight: 600; color: var(--color-primary-700); }
        .chrome-footer { font-size: 0.875rem; color: var(--color-base-500); }
        """;

    /// <summary>
    /// Assemblies beyond the DocSite template that carry additional
    /// <c>@@page</c> Razor components. Adding the app's entry assembly
    /// is what makes <see cref="Components.ExtraPage"/> visible to the router.
    /// </summary>
    public static Assembly[] BuildAdditionalRoutingAssemblies() =>
        [typeof(SiteChromeOverrides).Assembly];
}
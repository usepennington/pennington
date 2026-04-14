namespace ExtensibilityLabExample;

using System.Reflection;
using ExtensibilityLabExample.Components;
using Pennington.DocSite;
using Pennington.Routing;

/// <summary>
/// Static helpers that show how the <see cref="DocSiteOptions"/> slot
/// seams are wired. Each method body is a fence target for how-to
/// 2.3.70 <c>/how-to/extensibility/override-docsite-components</c>.
/// <para>
/// These helpers are <b>compile-only</b>. The main <c>Program.cs</c> in
/// this app uses the bare <c>AddPennington</c> host so the other six
/// extension points are visible raw; <see cref="DocSiteOptions"/> does
/// not apply to that host shape. A real DocSite app would call
/// <see cref="BuildDocSiteOptions"/> from its <c>AddDocSite(...)</c>
/// factory — see <c>examples/DocSiteKitchenSinkExample</c> for a
/// running example.
/// </para>
/// </summary>
public static class SiteChromeOverrides
{
    /// <summary>
    /// A populated <see cref="DocSiteOptions"/> that exercises all four
    /// slot seams the how-to covers: <see cref="DocSiteOptions.AdditionalHtmlHeadContent"/>,
    /// <see cref="DocSiteOptions.ExtraStyles"/>,
    /// <see cref="DocSiteOptions.HeaderContent"/> / <see cref="DocSiteOptions.FooterContent"/>
    /// (the string-HTML slots), and
    /// <see cref="DocSiteOptions.AdditionalRoutingAssemblies"/>.
    /// </summary>
    public static DocSiteOptions BuildDocSiteOptions() => new()
    {
        SiteTitle = "Extensibility Lab (DocSite variant)",
        Description = "DocSite wiring shape referenced by the bare-host sibling.",
        AdditionalHtmlHeadContent = BuildHtmlHeadContent(),
        ExtraStyles = BuildExtraStyles(),
        HeaderContent = """<span class="chrome-header" data-extensibility-lab="docsite-header">Custom header chrome</span>""",
        FooterContent = """<span class="chrome-footer" data-extensibility-lab="docsite-footer">Custom footer chrome</span>""",
        AdditionalRoutingAssemblies = BuildAdditionalRoutingAssemblies(),
    };

    /// <summary>
    /// The HTML string that ends up inside <c>&lt;head&gt;</c> on every
    /// page. In this example it is a hand-authored fragment — a
    /// <see cref="ExtraHeadFragment"/> Razor component's serialized
    /// output — so a how-to can show both the literal-string pattern
    /// and a component-rendered variation.
    /// </summary>
    public static string BuildHtmlHeadContent() => """
        <meta name="x-extensibility-lab-head" content="extra-head-fragment">
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
    /// is what makes <see cref="ExtraPage"/> visible to the router.
    /// </summary>
    public static Assembly[] BuildAdditionalRoutingAssemblies() =>
        [typeof(SiteChromeOverrides).Assembly];
}

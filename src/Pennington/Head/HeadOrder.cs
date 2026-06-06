namespace Pennington.Head;

/// <summary>
/// Named priority bands for <see cref="IHeadContributor.Order"/>. Contributors run lowest-first,
/// and on a <see cref="HeadTagKey"/> collision the lowest order wins — so page-level contributions
/// (lower bands) beat site-level defaults (higher bands). Bands replace the ad-hoc integer ordering
/// the HTML rewriters used, where unrelated writers silently collided on the same number.
/// </summary>
public static class HeadOrder
{
    /// <summary>Pre-paint essentials: charset/viewport and the theme bootstrap that must run before first paint.</summary>
    public const int Critical = 0;

    /// <summary>Stylesheets, scripts, font preloads, and preconnect hints.</summary>
    public const int Asset = 20;

    /// <summary>Page-authored or page-computed tags: title, description, per-page OpenGraph. Wins ties against site defaults.</summary>
    public const int Page = 40;

    /// <summary>Site-wide defaults: canonical, <c>og:site_name</c>, RSS/llms alternates, hreflang.</summary>
    public const int Site = 60;

    /// <summary>Discovery payloads: JSON-LD structured data and Standard Site verification links.</summary>
    public const int Discovery = 80;

    /// <summary>Diagnostic-only tags such as the dev-host meta used by live reload.</summary>
    public const int Diagnostic = 100;
}

namespace Pennington.DocSite;

/// <summary>Well-known keys for <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/> shared across DocSite and its integrations.</summary>
public static class DocSiteHttpContextKeys
{
    /// <summary>Pre-rewrite public request path. Middleware that rewrites <see cref="Microsoft.AspNetCore.Http.HttpRequest.Path"/> for internal routing stashes the caller-visible path here so the layout can resolve the active area and TOC selection against the URL the user actually sees.</summary>
    public const string OriginalPath = "Pennington.DocSite.OriginalPath";
}
namespace BeyondClientWidgetExample;

using Pennington.DocSite;

/// <summary>
/// Builds the <see cref="DocSiteOptions"/> for the client-widget example. The
/// head content wires GLightbox from a CDN alongside this site's own init
/// script; the matching server-rendered tag lives in
/// <see cref="Components.ImageGallery"/>. Both are fence targets for how-to
/// <c>/how-to/rich-content/client-side-widget</c>.
/// </summary>
public static class GalleryWidget
{
    /// <summary>
    /// The populated options passed to <c>AddDocSite</c>: a single guides area
    /// plus the gallery widget's head content.
    /// </summary>
    public static DocSiteOptions BuildDocSiteOptions() => new()
    {
        SiteTitle = "Client Widget Example",
        SiteDescription = "Ships an image-gallery lightbox by composing a CDN script, the head seam, and a server-rendered Mdazor component.",
        AdditionalHtmlHeadContent = BuildGalleryHeadContent(),
        Areas =
        [
            new ContentArea("Guides", "guides"),
        ],
    };

    /// <summary>
    /// The HTML injected into every page's <c>&lt;head&gt;</c>: the GLightbox
    /// stylesheet and script from jsDelivr, then this site's own init script.
    /// GLightbox is MIT-licensed and has no runtime dependencies; the version is
    /// pinned so the build is reproducible. Both <c>&lt;script&gt;</c> tags use
    /// <c>defer</c>, so they execute in order — GLightbox first, then the init
    /// script that calls it. A site that builds offline should vendor these two
    /// files into <c>wwwroot</c> and point the tags at the local copies instead.
    /// </summary>
    public static string BuildGalleryHeadContent() => """
        <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/glightbox@3.3.1/dist/css/glightbox.min.css">
        <script src="https://cdn.jsdelivr.net/npm/glightbox@3.3.1/dist/js/glightbox.min.js" defer></script>
        <script src="/gallery.js" defer></script>
        """;
}

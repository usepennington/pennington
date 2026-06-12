namespace Pennington.Book;

using System.Collections.Immutable;
using System.Text;
using Pennington.Artifacts;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Artifact-tier façade over <see cref="BookArtifactService"/>: claims <c>/pdf/</c> and
/// <c>/book-preview/</c>, renders PDFs and live previews on demand in dev through core's
/// artifact router, and enumerates the PDFs for the static build. Preview routes are
/// resolvable but deliberately not enumerated — they exist for live print-CSS iteration only,
/// so build output stays PDF-only. Transient so each resolution captures the current
/// file-watched service.
/// </summary>
public sealed class BookArtifactContentService : IArtifactContentService
{
    private static readonly ImmutableList<ArtifactClaim> ClaimList =
    [
        new ArtifactClaim("book", new PrefixClaim(new UrlPath("/pdf/"), ".pdf"), "book PDFs"),
        new ArtifactClaim("book", new PrefixClaim(new UrlPath("/book-preview/")), "book print previews (dev only)"),
    ];

    private readonly BookArtifactService _service;

    /// <summary>Creates the façade over the given <see cref="BookArtifactService"/>.</summary>
    public BookArtifactContentService(BookArtifactService service) => _service = service;

    /// <inheritdoc/>
    public ImmutableList<ArtifactClaim> Claims => ClaimList;

    /// <inheritdoc/>
    public async Task<ArtifactContent?> ResolveAsync(string relativePath, CancellationToken cancellationToken)
    {
        if (relativePath.StartsWith("pdf/", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = await _service.GetPdfAsync(relativePath);
            return bytes is null ? null : new ArtifactContent(bytes, "application/pdf");
        }

        if (relativePath.StartsWith("book-preview/", StringComparison.OrdinalIgnoreCase))
        {
            var html = await _service.GetPreviewHtmlAsync(relativePath);
            return html is null ? null : new ArtifactContent(Encoding.UTF8.GetBytes(html), "text/html; charset=utf-8");
        }

        return null;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        await Task.CompletedTask;
        foreach (var artifact in _service.EnumerateArtifacts())
        {
            yield return new DiscoveredItem(
                new ContentRoute
                {
                    CanonicalPath = new UrlPath("/" + artifact.PdfPath),
                    OutputFile = new FilePath(artifact.PdfPath),
                    Locale = artifact.Locale ?? "",
                },
                new GeneratedSource("application/pdf"));
        }
    }
}

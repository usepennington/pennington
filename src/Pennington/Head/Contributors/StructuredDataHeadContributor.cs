namespace Pennington.Head;

using Infrastructure;
using StructuredData;

/// <summary>
/// Emits a <c>&lt;script type="application/ld+json"&gt;</c> block for every record whose front matter
/// implements <see cref="IHasStructuredData"/>, joining the request path to its
/// <see cref="Content.ContentRecord"/> and composing the canonical URL. Replaces the former
/// <c>StructuredDataHtmlRewriter</c>.
/// </summary>
internal sealed class StructuredDataHeadContributor : IHeadContributor
{
    private readonly PenningtonOptions _options;

    /// <summary>Creates the contributor from the site options.</summary>
    public StructuredDataHeadContributor(PenningtonOptions options) => _options = options;

    /// <inheritdoc/>
    public int Order => HeadOrder.Discovery;

    /// <inheritdoc/>
    public bool ShouldContribute(HeadContext context)
        => !string.IsNullOrEmpty(_options.CanonicalBaseUrl)
           && context.Record?.Metadata is IHasStructuredData;

    /// <inheritdoc/>
    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        if (context.Record?.Metadata is not IHasStructuredData hasData)
        {
            return Task.CompletedTask;
        }

        var dataContext = new StructuredDataContext
        {
            // Compose the canonical URL from the record's canonical path (trailing-slash normalized),
            // not the raw request path — so a dev-server request to /page (no slash) still emits the
            // same /page/ URL. The request path is only the registry join key.
            CanonicalUrl = Combine(_options.CanonicalBaseUrl!, context.Record.Route.CanonicalPath.EnsureTrailingSlash().Value),
            FallbackAuthorName = _options.StructuredDataAuthorName,
        };

        foreach (var entity in hasData.GetStructuredData(dataContext))
        {
            if (entity is null)
            {
                continue;
            }

            // JsonLdSerializer escapes </ so the JSON is safe as raw script-tag text content.
            head.AddRepeatable(new HeadTag(new ScriptTag
            {
                Type = "application/ld+json",
                InlineBody = JsonLdSerializer.Serialize(entity),
            }));
        }

        return Task.CompletedTask;
    }

    private static string Combine(string baseUrl, string path)
    {
        var trimmedBase = baseUrl.TrimEnd('/');
        var trimmedPath = path.StartsWith('/') ? path : "/" + path;
        return trimmedBase + trimmedPath;
    }
}

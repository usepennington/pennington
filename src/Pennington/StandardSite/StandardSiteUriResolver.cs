namespace Pennington.StandardSite;

using Content;

/// <summary>
/// Resolves Standard Site AT-URIs: the site-wide publication URI (config only) and a per-page
/// <c>site.standard.document</c> URI (joining the request path to the content registry, then
/// reading the page's rkey via <see cref="StandardSiteOptions.DocumentRkeyResolver"/>). Registered
/// transient so it captures the current file-watched <see cref="ContentRecordRegistry"/>.
/// </summary>
public sealed class StandardSiteUriResolver
{
    private readonly StandardSiteOptions _options;
    private readonly ContentRecordRegistry _records;

    /// <summary>Creates the resolver from the Standard Site options and the record registry.</summary>
    public StandardSiteUriResolver(StandardSiteOptions options, ContentRecordRegistry records)
    {
        _options = options;
        _records = records;
    }

    /// <summary>The publication AT-URI (<c>at://{Did}/site.standard.publication/{PublicationRkey}</c>).</summary>
    public string PublicationUri => AtUri.Build(_options.Did, "site.standard.publication", _options.PublicationRkey);

    /// <summary>
    /// The <c>site.standard.document</c> AT-URI for a request path, or <c>null</c> when the page
    /// resolves to no record or declares no rkey. Keys the registry on
    /// <c>fullPath.Trim('/')</c> — identical to the structured-data join.
    /// </summary>
    public async Task<string?> DocumentUriAsync(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }

        var snapshot = await _records.GetSnapshotAsync();
        if (snapshot.TryGetValue(fullPath.Trim('/'), out var record)
            && _options.DocumentRkeyResolver(record) is { Length: > 0 } rkey)
        {
            return AtUri.Build(_options.Did, "site.standard.document", rkey);
        }

        return null;
    }
}

namespace Pennington.IntegrationTests.DocsSite;

using System.Text.Json;
using DeweySearch;

/// <summary>
/// One decoded row of the search document table: what the client resolves a hit to.
/// </summary>
internal sealed record IndexedDoc(int Id, string Url, string Title, IReadOnlyList<string> Crumbs, int Priority);

/// <summary>
/// Reads the emitted search artifacts back into document rows for assertions.
/// <para>
/// DeweySearch 0.2.0 splits the index hot/cold: <c>index.json</c> is the hot entrypoint
/// (BM25 stats, per-doc priorities, the crumb-label dictionary, shard keys) and the document
/// table lives in cold <c>d-{n}.json</c> shards the client fetches on demand. Urls and titles
/// inside a shard are front-coded against the previous row — the leading character carries the
/// shared-prefix length as <c>c - '0'</c>.
/// </para>
/// <para>
/// This is the only place in the test suite that knows that wire format. Tests assert on
/// <see cref="IndexedDoc"/> so a future DeweySearch layout change lands here rather than across
/// every search assertion. The manifest and shards deserialize into DeweySearch's own
/// <see cref="IndexManifest"/> / <see cref="DocShard"/> types, so the field-name contract stays
/// the package's to define.
/// </para>
/// </summary>
internal static class SearchIndexReader
{
    public static async Task<IReadOnlyList<IndexedDoc>> LoadAsync(
        HttpClient client, string locale = "en", CancellationToken cancellationToken = default)
    {
        var manifestJson = await client.GetStringAsync($"/search/{locale}/index.json", cancellationToken);
        var manifest = JsonSerializer.Deserialize<IndexManifest>(manifestJson)
            ?? throw new InvalidOperationException($"search/{locale}/index.json did not deserialize");

        var docs = new List<IndexedDoc>(manifest.DocCount);
        var shardCount = (manifest.DocCount + manifest.DocShardSize - 1) / manifest.DocShardSize;

        for (var s = 0; s < shardCount; s++)
        {
            var shardJson = await client.GetStringAsync($"/search/{locale}/d-{s}.json", cancellationToken);
            var shard = JsonSerializer.Deserialize<DocShard>(shardJson)
                ?? throw new InvalidOperationException($"search/{locale}/d-{s}.json did not deserialize");

            var urls = FrontDecode(shard.Urls);
            var titles = FrontDecode(shard.Titles);

            for (var i = 0; i < urls.Count; i++)
            {
                var id = shard.Offset + i;
                if (id >= manifest.DocCount)
                {
                    break;
                }

                var crumbs = shard.Crumbs is { } c && i < c.Length && c[i] is { } row
                    ? row.Select(ix => manifest.CrumbLabels[ix]).ToArray()
                    : [];

                var priority = manifest.Priorities is { } pri && id < pri.Length
                    ? pri[id]
                    : manifest.DefaultPriority;

                docs.Add(new IndexedDoc(id, urls[i], titles[i], crumbs, priority));
            }
        }

        return docs;
    }

    private static IReadOnlyList<string> FrontDecode(IReadOnlyList<string> encoded)
    {
        var result = new List<string>(encoded.Count);
        var previous = "";
        foreach (var entry in encoded)
        {
            // Leading char is the shared-prefix length against the previous entry.
            var shared = entry[0] - '0';
            var value = string.Concat(previous.AsSpan(0, shared), entry.AsSpan(1));
            result.Add(value);
            previous = value;
        }

        return result;
    }
}

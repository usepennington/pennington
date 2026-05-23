namespace Pennington.Search;

using System.Collections.Immutable;
using Content;
using Routing;

/// <summary>
/// Thin <see cref="IContentEmitter"/> adapter that writes the sharded search artifacts
/// (entrypoint, term shards, and per-page fragments) into the static build output.
/// Registered as transient so each resolution picks up the current file-watched
/// <see cref="SearchArtifactService"/>.
/// </summary>
public sealed class SearchArtifactEmitter : IContentEmitter
{
    private readonly SearchArtifactService _service;

    /// <summary>Creates an emitter backed by the given <see cref="SearchArtifactService"/>.</summary>
    public SearchArtifactEmitter(SearchArtifactService service) => _service = service;

    /// <inheritdoc/>
    public async Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
    {
        var files = await _service.GetArtifactFilesAsync();
        var builder = ImmutableList.CreateBuilder<ContentToCreate>();
        foreach (var (path, bytes) in files)
        {
            var captured = bytes;
            builder.Add(new ContentToCreate(
                new FilePath(path),
                () => Task.FromResult(captured),
                "application/json"));
        }

        return builder.ToImmutable();
    }
}

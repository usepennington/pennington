namespace Pennington.Book;

using System.Collections.Immutable;
using Content;
using Routing;

/// <summary>
/// Thin <see cref="IContentEmitter"/> adapter that writes each book's PDF into the static build output.
/// Registered transient so each resolution picks up the current file-watched <see cref="BookArtifactService"/>.
/// The content generator is lazy — paths are enumerated cheaply, and Chromium only runs when the build
/// pass invokes the generator (Phase 5 wraps that call in try/catch, so a Chromium failure becomes a
/// BuildReport error rather than a crashed build).
/// </summary>
public sealed class BookContentEmitter : IContentEmitter
{
    private readonly BookArtifactService _service;

    /// <summary>Creates an emitter backed by the given <see cref="BookArtifactService"/>.</summary>
    public BookContentEmitter(BookArtifactService service) => _service = service;

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
    {
        var builder = ImmutableList.CreateBuilder<ContentToCreate>();
        foreach (var artifact in _service.EnumerateArtifacts())
        {
            var path = artifact.PdfPath;
            builder.Add(new ContentToCreate(
                new FilePath(path),
                async () => await _service.GetPdfAsync(path)
                    ?? throw new InvalidOperationException($"Book artifact '{path}' produced no PDF."),
                "application/pdf"));
        }

        return Task.FromResult(builder.ToImmutable());
    }
}

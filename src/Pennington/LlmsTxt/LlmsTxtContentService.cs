namespace Pennington.LlmsTxt;

using System.Collections.Immutable;
using System.Text;
using Content;
using Pipeline;
using Routing;

/// <summary>
/// Thin <see cref="IContentService"/> adapter that delegates to <see cref="LlmsTxtService"/>
/// for file generation during static builds. Registered as transient so each
/// resolution from the content-service enumerable picks up the current
/// file-watched <see cref="LlmsTxtService"/>.
/// </summary>
public sealed class LlmsTxtContentService : IContentService
{
    private readonly LlmsTxtService _service;
    private readonly LlmsTxtOptions _options;

    /// <summary>Creates a content service that emits llms.txt and stripped markdown files produced by the given <see cref="LlmsTxtService"/>.</summary>
    public LlmsTxtContentService(LlmsTxtService service, LlmsTxtOptions options)
    {
        _service = service;
        _options = options;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        await Task.CompletedTask;
        yield break;
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc/>
    public async Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
    {
        var builder = ImmutableList.CreateBuilder<ContentToCreate>();

        // llms.txt index
        builder.Add(new ContentToCreate(
            new FilePath("llms.txt"),
            async () => Encoding.UTF8.GetBytes(await _service.GetLlmsTxtAsync()),
            "text/plain"));

        // Individual stripped markdown files
        var markdownFiles = await _service.GetMarkdownFilesAsync();
        foreach (var file in markdownFiles)
        {
            var captured = file;
            builder.Add(new ContentToCreate(
                captured.OutputPath,
                () => Task.FromResult(captured.Content),
                "text/markdown"));
        }

        // Per-subtree {prefix}llms.txt files (split out of the front door)
        var subtreeFiles = await _service.GetSubtreeFilesAsync();
        foreach (var file in subtreeFiles)
        {
            var captured = file;
            builder.Add(new ContentToCreate(
                captured.OutputPath,
                () => Task.FromResult(captured.Content),
                "text/plain"));
        }

        // Optional full concatenated file
        if (_options.GenerateFullFile)
        {
            builder.Add(new ContentToCreate(
                new FilePath("llms-full.txt"),
                async () => Encoding.UTF8.GetBytes(await _service.GetLlmsFullTxtAsync() ?? ""),
                "text/plain"));
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    /// <inheritdoc/>
    public string DefaultSectionLabel => "";

    /// <inheritdoc/>
    public int SearchPriority => 0;
}
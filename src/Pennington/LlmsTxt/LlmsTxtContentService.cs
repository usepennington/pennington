namespace Pennington.LlmsTxt;

using System.Collections.Immutable;
using System.Text;
using Content;
using Infrastructure;
using Pipeline;
using Routing;

/// <summary>
/// Thin <see cref="IContentService"/> adapter that delegates to <see cref="LlmsTxtService"/>
/// for file generation during static builds. Reads the current <see cref="LlmsTxtService"/>
/// via its <see cref="FileWatchDependencyFactory{T}"/> so file-change rebuilds propagate.
/// </summary>
public sealed class LlmsTxtContentService : IContentService
{
    private readonly FileWatchDependencyFactory<LlmsTxtService> _serviceFactory;
    private readonly LlmsTxtOptions _options;

    /// <summary>Creates a content service that emits llms.txt and stripped markdown files produced by the current <see cref="LlmsTxtService"/>.</summary>
    public LlmsTxtContentService(FileWatchDependencyFactory<LlmsTxtService> serviceFactory, LlmsTxtOptions options)
    {
        _serviceFactory = serviceFactory;
        _options = options;
    }

    private LlmsTxtService Service => _serviceFactory.Current;

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
            async () => Encoding.UTF8.GetBytes(await Service.GetLlmsTxtAsync()),
            "text/plain"));

        // Individual stripped markdown files
        var markdownFiles = await Service.GetMarkdownFilesAsync();
        foreach (var file in markdownFiles)
        {
            var captured = file;
            builder.Add(new ContentToCreate(
                captured.OutputPath,
                () => Task.FromResult(captured.Content),
                "text/markdown"));
        }

        // Optional full concatenated file
        if (_options.GenerateFullFile)
        {
            builder.Add(new ContentToCreate(
                new FilePath("llms-full.txt"),
                async () => Encoding.UTF8.GetBytes(await Service.GetLlmsFullTxtAsync() ?? ""),
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
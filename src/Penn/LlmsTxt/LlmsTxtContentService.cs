namespace Penn.LlmsTxt;

using System.Collections.Immutable;
using System.Text;
using Penn.Content;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Thin <see cref="IContentService"/> adapter that delegates to <see cref="LlmsTxtService"/>
/// for file generation during static builds.
/// </summary>
public sealed class LlmsTxtContentService : IContentService
{
    private readonly LlmsTxtService _service;
    private readonly LlmsTxtOptions _options;

    public LlmsTxtContentService(LlmsTxtService service, LlmsTxtOptions options)
    {
        _service = service;
        _options = options;
    }

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        await Task.CompletedTask;
        yield break;
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

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

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    public string DefaultSection => "";
    public int SearchPriority => 0;
}

namespace Pennington.Pipeline;

/// <summary>
/// The single <see cref="IContentParser"/> the pipeline resolves: routes each discovered item to the
/// parser registered for its format (see <see cref="ContentFormatRegistry"/>) and stamps the resolved
/// format onto the returned <see cref="ParsedItem"/> so <see cref="DispatchingContentRenderer"/> can
/// dispatch on it.
/// </summary>
public sealed class DispatchingContentParser : IContentParser
{
    private readonly ContentFormatRegistry _registry;
    private readonly IServiceProvider _services;

    /// <summary>Creates the dispatcher from the format registry and the service provider it resolves parsers from.</summary>
    public DispatchingContentParser(ContentFormatRegistry registry, IServiceProvider services)
    {
        _registry = registry;
        _services = services;
    }

    /// <inheritdoc/>
    public async Task<ContentItem> ParseAsync(DiscoveredItem item)
    {
        var format = item.Source.Value switch
        {
            FileSource fs => fs.Format,
            LlmsOnlySource llms => llms.Format,
            _ => null,
        };

        if (format is null)
        {
            return new FailedItem(item.Route,
                new ContentError("Unsupported content source type for parser"));
        }

        if (!_registry.TryGetParser(format, out var factory))
        {
            return new FailedItem(item.Route,
                new ContentError($"No parser registered for content format '{format}'"));
        }

        var result = await factory(_services).ParseAsync(item);

        // Stamp the resolved format so the dispatching renderer routes this item correctly,
        // regardless of whether the inner parser set it.
        if (result.Value is ParsedItem parsed)
        {
            return parsed with { Format = format };
        }

        return result;
    }
}

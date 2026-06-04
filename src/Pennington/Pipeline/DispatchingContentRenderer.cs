namespace Pennington.Pipeline;

/// <summary>
/// The single <see cref="IContentRenderer"/> the pipeline resolves: routes each parsed item to the
/// renderer registered for its <see cref="ParsedItem.Format"/> (see <see cref="ContentFormatRegistry"/>).
/// </summary>
public sealed class DispatchingContentRenderer : IContentRenderer
{
    private readonly ContentFormatRegistry _registry;
    private readonly IServiceProvider _services;

    /// <summary>Creates the dispatcher from the format registry and the service provider it resolves renderers from.</summary>
    public DispatchingContentRenderer(ContentFormatRegistry registry, IServiceProvider services)
    {
        _registry = registry;
        _services = services;
    }

    /// <inheritdoc/>
    public async Task<ContentItem> RenderAsync(ParsedItem item)
    {
        if (!_registry.TryGetRenderer(item.Format, out var factory))
        {
            return new FailedItem(item.Route,
                new ContentError($"No renderer registered for content format '{item.Format}'"));
        }

        return await factory(_services).RenderAsync(item);
    }
}

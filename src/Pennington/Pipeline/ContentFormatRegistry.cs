namespace Pennington.Pipeline;

/// <summary>
/// Maps content-format keys (e.g. <c>"markdown"</c>, <c>"cook"</c>) to the factories that build the
/// format's <see cref="IContentParser"/> and <see cref="IContentRenderer"/>. Populated once while
/// <c>AddPennington</c> wires services, then read by <see cref="DispatchingContentParser"/> and
/// <see cref="DispatchingContentRenderer"/> at request time. Format keys are matched
/// case-insensitively.
/// </summary>
public sealed class ContentFormatRegistry
{
    private readonly Dictionary<string, Func<IServiceProvider, IContentParser>> _parsers =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Func<IServiceProvider, IContentRenderer>> _renderers =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers (overwriting) the parser and renderer factories for a format key.</summary>
    /// <param name="format">Format key the dispatchers route on.</param>
    /// <param name="parser">Resolves the format's parser from the request scope.</param>
    /// <param name="renderer">Resolves the format's renderer from the request scope.</param>
    public void Register(
        string format,
        Func<IServiceProvider, IContentParser> parser,
        Func<IServiceProvider, IContentRenderer> renderer)
    {
        _parsers[format] = parser;
        _renderers[format] = renderer;
    }

    /// <summary>Gets the parser factory registered for <paramref name="format"/>, if any.</summary>
    public bool TryGetParser(string format, out Func<IServiceProvider, IContentParser> factory)
        => _parsers.TryGetValue(format, out factory!);

    /// <summary>Gets the renderer factory registered for <paramref name="format"/>, if any.</summary>
    public bool TryGetRenderer(string format, out Func<IServiceProvider, IContentRenderer> factory)
        => _renderers.TryGetValue(format, out factory!);
}

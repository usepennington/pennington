namespace Pennington.Content;

using System.Collections.Immutable;
using Routing;

/// <summary>Case types for <see cref="ContentChangeImpact"/>.</summary>
public static class ContentChangeImpactCases
{
    /// <summary>File is outside the service's scope — no routes are affected.</summary>
    public sealed record None;

    /// <summary>
    /// The change affects an unknown / unbounded set of routes (a rename without an
    /// old-path signal, a folder-metadata sidecar edit, a layout change). Consumers
    /// drop their full cache.
    /// </summary>
    public sealed record Wildcard;

    /// <summary>The change affects the listed routes; consumers evict per-route.</summary>
    /// <param name="Affected">Routes that need re-rendering / re-fetching.</param>
    public sealed record Routes(ImmutableArray<ContentRoute> Affected);
}

/// <summary>
/// What an <see cref="IContentService"/> reports as affected by a single
/// <see cref="Infrastructure.FileChangeNotification"/>. Used by file-watched caches
/// to drive granular invalidation.
/// </summary>
#if NET11_0_OR_GREATER
public union ContentChangeImpact(
    ContentChangeImpactCases.None,
    ContentChangeImpactCases.Wildcard,
    ContentChangeImpactCases.Routes);
#else
[System.Runtime.CompilerServices.Union]
public readonly struct ContentChangeImpact : System.Runtime.CompilerServices.IUnion
{
    /// <summary>Wrapped case instance; inspect via pattern matching on the case types.</summary>
    public object? Value { get; }
    /// <summary>Wraps a <see cref="ContentChangeImpactCases.None"/>.</summary>
    public ContentChangeImpact(ContentChangeImpactCases.None value) { Value = value; }
    /// <summary>Wraps a <see cref="ContentChangeImpactCases.Wildcard"/>.</summary>
    public ContentChangeImpact(ContentChangeImpactCases.Wildcard value) { Value = value; }
    /// <summary>Wraps a <see cref="ContentChangeImpactCases.Routes"/>.</summary>
    public ContentChangeImpact(ContentChangeImpactCases.Routes value) { Value = value; }
    /// <summary>Implicit conversion from <see cref="ContentChangeImpactCases.None"/>.</summary>
    public static implicit operator ContentChangeImpact(ContentChangeImpactCases.None value) => new(value);
    /// <summary>Implicit conversion from <see cref="ContentChangeImpactCases.Wildcard"/>.</summary>
    public static implicit operator ContentChangeImpact(ContentChangeImpactCases.Wildcard value) => new(value);
    /// <summary>Implicit conversion from <see cref="ContentChangeImpactCases.Routes"/>.</summary>
    public static implicit operator ContentChangeImpact(ContentChangeImpactCases.Routes value) => new(value);
}
#endif

namespace Pennington.Content;

using System.Collections.Immutable;
using Routing;

/// <summary>
/// What an <see cref="IContentService"/> reports as affected by a single
/// <see cref="Infrastructure.FileChangeNotification"/>. <see cref="AffectedRoutes"/>
/// is <c>null</c> for a wildcard (consumers drop their full cache), empty for no impact,
/// and populated for per-route eviction.
/// </summary>
/// <param name="AffectedRoutes">Routes that need re-rendering, or <c>null</c> to signal a wildcard.</param>
public sealed record ContentChangeImpact(ImmutableArray<ContentRoute>? AffectedRoutes)
{
    /// <summary>No routes affected — the change is outside this service's scope.</summary>
    public static ContentChangeImpact None { get; } = new(ImmutableArray<ContentRoute>.Empty);

    /// <summary>Affects an unknown / unbounded set of routes; consumers should drop their full cache.</summary>
    public static ContentChangeImpact Wildcard { get; } = new((ImmutableArray<ContentRoute>?)null);

    /// <summary>Affects the given routes; consumers evict per-route.</summary>
    public static ContentChangeImpact Routes(ImmutableArray<ContentRoute> routes) => new(routes);
}

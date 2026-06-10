namespace Pennington.Generation;

using System.Collections.Immutable;
using Infrastructure;
using Localization;
using Routing;

/// <summary>
/// Inputs handed to <see cref="IRenderedAuditor.AuditAsync"/>: the routes to fetch
/// plus a delegate that returns each route's post-pipeline rendered HTML. Unlike
/// <see cref="BuildAuditContext"/>, the page set is the full generated route set —
/// every discovered HTML page — so rendered checks cover routes that never appear
/// in navigation (the homepage and other Razor-only pages).
/// </summary>
/// <param name="Pages">Every generated HTML route, from full content discovery rather than the navigation TOC.</param>
/// <param name="Localization">Configured locales and the default-locale code.</param>
/// <param name="GetRenderedHtmlAsync">
/// Fetches the rendered HTML for a <paramref name="Pages"/> route through the
/// live application pipeline. Returns <c>null</c> when the route does not
/// resolve (404) so the caller can filter rather than handle exceptions.
/// </param>
public sealed record RenderedAuditContext(
    ImmutableList<ContentRoute> Pages,
    LocalizationOptions Localization,
    Func<ContentRoute, CancellationToken, Task<string?>> GetRenderedHtmlAsync
);
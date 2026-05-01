namespace Pennington.Generation;

using System.Collections.Immutable;
using Content;
using Infrastructure;
using Routing;

/// <summary>
/// Inputs handed to <see cref="IRenderedAuditor.AuditAsync"/>. Mirrors
/// <see cref="BuildAuditContext"/> and adds a fetcher delegate for retrieving
/// post-pipeline rendered HTML by route.
/// </summary>
/// <param name="Pages">All TOC entries from every registered <see cref="IContentService"/>.</param>
/// <param name="Localization">Configured locales and the default-locale code.</param>
/// <param name="GetRenderedHtmlAsync">
/// Fetches the rendered HTML for <paramref name="Pages"/> entries through the
/// live application pipeline. Returns <c>null</c> when the route does not
/// resolve (404) so the caller can filter rather than handle exceptions.
/// </param>
public sealed record RenderedAuditContext(
    ImmutableList<ContentTocItem> Pages,
    LocalizationOptions Localization,
    Func<ContentRoute, CancellationToken, Task<string?>> GetRenderedHtmlAsync
);

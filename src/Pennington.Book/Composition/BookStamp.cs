namespace Pennington.Book.Composition;

/// <summary>
/// Provenance stamped into a composed book: the cover's version line and the colophon page.
/// When no stamp is supplied to <see cref="BookComposer.Compose"/>, both are omitted.
/// </summary>
/// <param name="Version">Host site version (informational version with build metadata trimmed); null omits the version lines.</param>
/// <param name="GeneratedAt">Generation timestamp; the colophon prints it as month + year.</param>
/// <param name="Locale">Locale code driving the <c>lang</c> attribute, translation lookups, and date formatting; null means the default locale.</param>
public sealed record BookStamp(string? Version, DateTimeOffset GeneratedAt, string? Locale);

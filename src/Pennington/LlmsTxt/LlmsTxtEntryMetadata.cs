namespace Pennington.LlmsTxt;

/// <summary>
/// Endpoint metadata that surfaces a <c>MapGet</c> route in <c>llms.txt</c>
/// without producing an HTML page. Attached to a route via
/// <see cref="LlmsTxtEndpointExtensions.WithLlmsTxtEntry{TBuilder}"/>; consumed
/// by <see cref="LlmsTxtService"/> to fold the endpoint's URL into the
/// generated index alongside discovered markdown entries.
/// </summary>
/// <param name="Title">Display title shown in the <c>llms.txt</c> front door.</param>
/// <param name="Description">Optional one-line description rendered after the link.</param>
public sealed record LlmsTxtEntryMetadata(string Title, string? Description = null);
namespace Pennington.LlmsTxt;

using Microsoft.AspNetCore.Builder;

/// <summary>
/// Endpoint convention extensions for opting a <c>MapGet</c> route into the
/// generated <c>llms.txt</c> index.
/// </summary>
public static class LlmsTxtEndpointExtensions
{
    /// <summary>
    /// Registers the endpoint as an <c>llms.txt</c> entry. The route's URL is
    /// the link target in the generated index — typical use is a
    /// <c>MapGet("/_llms/{slug}.md", () =&gt; Results.Text(markdown, "text/markdown"))</c>
    /// that returns markdown directly. No HTML page is manufactured, and no
    /// sidecar is generated; the user-supplied response IS the content
    /// <c>llms.txt</c> consumers fetch.
    /// </summary>
    /// <typeparam name="TBuilder">Convention builder type, typically <see cref="RouteHandlerBuilder"/>.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="title">Display title for the entry.</param>
    /// <param name="description">Optional description rendered after the link.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static TBuilder WithLlmsTxtEntry<TBuilder>(
        this TBuilder builder,
        string title,
        string? description = null)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new LlmsTxtEntryMetadata(title, description));
        return builder;
    }
}
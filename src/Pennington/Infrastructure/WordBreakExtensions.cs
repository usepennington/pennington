namespace Pennington.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Opt-in registration for word-break typography.
/// </summary>
public static class WordBreakExtensions
{
    /// <summary>
    /// Registers <see cref="WordBreakHtmlRewriter"/> in the shared HTML
    /// rewriting pipeline, so long identifiers in the configured elements get
    /// <c>&lt;wbr&gt;</c> break opportunities without an extra DOM parse.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration of the selector and break behavior.</param>
    /// <returns>The service collection, for chaining.</returns>
    public static IServiceCollection AddWordBreak(
        this IServiceCollection services,
        Action<WordBreakOptions>? configure = null)
    {
        var options = new WordBreakOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IHtmlResponseRewriter, WordBreakHtmlRewriter>();
        return services;
    }
}

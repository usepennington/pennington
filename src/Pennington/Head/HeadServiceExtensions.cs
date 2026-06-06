namespace Pennington.Head;

using Infrastructure;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Registration helpers for the head-composition pipeline.</summary>
public static class HeadServiceExtensions
{
    /// <summary>
    /// Registers the head composition rewriter. Inert until at least one <see cref="IHeadContributor"/>
    /// is also registered, so adding this on its own leaves head output byte-identical.
    /// </summary>
    public static IServiceCollection AddHead(this IServiceCollection services)
    {
        services.AddTransient<IHtmlResponseRewriter, HeadCompositionHtmlRewriter>();

        // Built-in core contributors. Each self-gates on its precondition (e.g. CanonicalBaseUrl),
        // so registering them unconditionally matches the always-registered rewriters they replace.
        services.AddHeadContributor<CanonicalHeadContributor>();
        services.AddHeadContributor<StructuredDataHeadContributor>();
        return services;
    }

    /// <summary>
    /// Registers a single head contributor. Transient so contributors capturing a file-watched
    /// dependency (e.g. the content registry) pick up the current instance per request.
    /// </summary>
    public static IServiceCollection AddHeadContributor<T>(this IServiceCollection services)
        where T : class, IHeadContributor
        => services.AddTransient<IHeadContributor, T>();
}

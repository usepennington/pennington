namespace Pennington.Pipeline;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// DI helpers for swapping the registered <see cref="IContentRenderer"/>.
/// </summary>
public static class ContentRendererServiceExtensions
{
    /// <summary>
    /// Replaces every registered <see cref="IContentRenderer"/> with <typeparamref name="TNew"/>,
    /// resolved through DI as a transient. The <typeparamref name="TOld"/> type parameter
    /// documents the renderer being swapped out — it is informational and unused at runtime,
    /// but lets the call site read as "replace TOld with TNew".
    /// </summary>
    /// <remarks>
    /// Use after <c>AddPennington</c> / <c>AddDocSite</c> / <c>AddBlogSite</c> to install a
    /// custom renderer (typically a decorator) without relying on last-wins registration order.
    /// </remarks>
    public static IServiceCollection ReplaceContentRenderer<TOld, TNew>(this IServiceCollection services)
        where TOld : class, IContentRenderer
        where TNew : class, IContentRenderer
    {
        services.RemoveAll<IContentRenderer>();
        services.AddTransient<IContentRenderer, TNew>();
        return services;
    }

    /// <summary>
    /// Replaces every registered <see cref="IContentRenderer"/> with one produced by
    /// <paramref name="factory"/>. Use this overload when the new renderer takes ctor
    /// arguments DI cannot resolve (e.g. a version string or per-site constant).
    /// </summary>
    public static IServiceCollection ReplaceContentRenderer<TOld, TNew>(
        this IServiceCollection services,
        Func<IServiceProvider, TNew> factory)
        where TOld : class, IContentRenderer
        where TNew : class, IContentRenderer
    {
        ArgumentNullException.ThrowIfNull(factory);
        services.RemoveAll<IContentRenderer>();
        services.AddTransient<IContentRenderer>(factory);
        return services;
    }
}
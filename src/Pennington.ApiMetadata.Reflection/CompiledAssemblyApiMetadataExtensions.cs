namespace Pennington.ApiMetadata.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pennington.ApiMetadata;

/// <summary>DI extension that registers a reflection-backed <see cref="IApiMetadataProvider"/>.</summary>
public static class CompiledAssemblyApiMetadataExtensions
{
    /// <summary>
    /// Registers <see cref="CompiledAssemblyApiMetadataProvider"/> as a keyed
    /// <see cref="IApiMetadataProvider"/> under <paramref name="name"/>. Call once per
    /// library you want to document — each call builds its own
    /// <see cref="System.Reflection.MetadataLoadContext"/> and xmldoc index scoped to
    /// the supplied <see cref="CompiledAssemblyApiOptions.AssemblyDirectories"/>. The
    /// shared <c>IXmlDocParser</c> / <c>IXmlDocHtmlRenderer</c>
    /// services are registered once (idempotent).
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="name">Registration name. Pair with the matching <c>AddApiReference(name, …)</c> call. Defaults to <c>"default"</c>.</param>
    /// <param name="configure">Required configuration — at minimum add one entry to <c>AssemblyDirectories</c>.</param>
    public static IServiceCollection AddApiMetadataFromCompiledAssembly(
        this IServiceCollection services,
        string name,
        Action<CompiledAssemblyApiOptions> configure)
    {
        var options = new CompiledAssemblyApiOptions();
        configure(options);
        if (options.AssemblyDirectories.Count == 0 && options.AssemblyFiles.Count == 0)
        {
            throw new InvalidOperationException(
                "AddApiMetadataFromCompiledAssembly requires at least one entry in AssemblyDirectories or AssemblyFiles.");
        }

        services.TryAddSingleton<IXmlDocParser, XmlDocParser>();
        services.TryAddSingleton<IXmlDocHtmlRenderer, XmlDocHtmlRenderer>();
        services.AddKeyedSingleton(name, options);
        services.AddKeyedSingleton(name, (sp, key) =>
            ActivatorUtilities.CreateInstance<CompiledAssemblyApiMetadataProvider>(sp,
                sp.GetRequiredKeyedService<CompiledAssemblyApiOptions>(key)));
        services.AddKeyedSingleton<IApiMetadataProvider>(name, (sp, key) =>
            sp.GetRequiredKeyedService<CompiledAssemblyApiMetadataProvider>(key));
        return services;
    }

    /// <summary>
    /// Convenience overload: registers under the <c>"default"</c> name for sites
    /// documenting a single library.
    /// </summary>
    public static IServiceCollection AddApiMetadataFromCompiledAssembly(
        this IServiceCollection services,
        Action<CompiledAssemblyApiOptions> configure)
        => services.AddApiMetadataFromCompiledAssembly("default", configure);
}
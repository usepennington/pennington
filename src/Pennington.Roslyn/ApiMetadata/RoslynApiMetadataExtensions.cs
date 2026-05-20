namespace Pennington.Roslyn.ApiMetadata;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pennington.ApiMetadata;

/// <summary>DI extension that registers a Roslyn-backed <see cref="IApiMetadataProvider"/>.</summary>
public static class RoslynApiMetadataExtensions
{
    /// <summary>
    /// Registers <see cref="RoslynApiMetadataProvider"/> as a keyed
    /// <see cref="IApiMetadataProvider"/> under <paramref name="name"/>. Call once per
    /// named reference tree. Requires <c>AddPenningtonRoslyn</c> to have been called
    /// first with a configured <c>SolutionPath</c>, since the provider reads the live
    /// workspace.
    /// </summary>
    /// <remarks>
    /// When <paramref name="name"/> is <c>"default"</c>, <see cref="ApiReferenceOptions"/> is
    /// also exposed as a non-keyed singleton so consumers can inject it via plain
    /// constructor parameters without the keyed-resolve dance.
    /// </remarks>
    /// <param name="services">Service collection.</param>
    /// <param name="name">Registration name. Pair with the matching <c>AddApiReference(name, …)</c> call. Defaults to <c>"default"</c>.</param>
    /// <param name="configure">Optional project/type filter configuration.</param>
    public static IServiceCollection AddApiMetadataFromRoslyn(
        this IServiceCollection services,
        string name = "default",
        Action<ApiReferenceOptions>? configure = null)
    {
        var options = new ApiReferenceOptions();
        configure?.Invoke(options);
        services.AddKeyedSingleton(name, options);
        if (name == "default")
        {
            services.TryAddSingleton<ApiReferenceOptions>(sp =>
                sp.GetRequiredKeyedService<ApiReferenceOptions>("default"));
        }
        services.AddKeyedSingleton<RoslynApiMetadataProvider>(name, (sp, key) =>
            ActivatorUtilities.CreateInstance<RoslynApiMetadataProvider>(sp,
                sp.GetRequiredKeyedService<ApiReferenceOptions>(key)));
        services.AddKeyedSingleton<IApiMetadataProvider>(name, (sp, key) =>
            sp.GetRequiredKeyedService<RoslynApiMetadataProvider>(key));
        return services;
    }
}
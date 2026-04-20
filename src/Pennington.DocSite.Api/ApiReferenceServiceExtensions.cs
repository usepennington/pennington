namespace Pennington.DocSite.Api;

using Components.Reference;
using Mdazor;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Content;

/// <summary>
/// DI extension methods for registering the API reference package.
/// </summary>
public static class ApiReferenceServiceExtensions
{
    /// <summary>
    /// Registers the API reference package: auto-publishes <c>/reference/api/</c>
    /// pages for every public type in the configured projects, and registers the
    /// <c>ApiMemberTable</c>, <c>ApiSummary</c>, <c>ExtensionMethods</c>, etc.
    /// Mdazor components for inline use in markdown.
    /// </summary>
    /// <remarks>
    /// Call after <see cref="Pennington.DocSite.DocSiteServiceExtensions.AddDocSite"/>
    /// and <see cref="Pennington.Roslyn.RoslynExtensions.AddPenningtonRoslyn"/>:
    /// the extension mutates the registered <see cref="DocSiteOptions"/> singleton
    /// to include this library's assembly in
    /// <see cref="DocSiteOptions.AdditionalRoutingAssemblies"/>, so Blazor's
    /// runtime router can resolve the package's <c>@page</c> components.
    /// </remarks>
    public static IServiceCollection AddApiReference(
        this IServiceCollection services,
        Action<ApiReferenceOptions>? configure = null)
    {
        var options = new ApiReferenceOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddSingleton<ApiReferenceIndex>();
        services.AddSingleton<ExtensionMethodIndex>();
        services.AddSingleton<IContentService, ApiReferenceContentService>();

        services.AddMdazorComponent<ApiMemberTable>()
                .AddMdazorComponent<ApiMemberList>()
                .AddMdazorComponent<ApiParameterTable>()
                .AddMdazorComponent<ApiSummary>()
                .AddMdazorComponent<ApiReturns>()
                .AddMdazorComponent<ApiRemarks>()
                .AddMdazorComponent<ApiSeeAlso>()
                .AddMdazorComponent<ApiDefinitionList>()
                .AddMdazorComponent<FieldList>()
                .AddMdazorComponent<Field>()
                .AddMdazorComponent<ExtensionMethods>();

        AppendRoutingAssembly(services);

        return services;
    }

    private static void AppendRoutingAssembly(IServiceCollection services)
    {
        var asm = typeof(ApiReferenceServiceExtensions).Assembly;

        for (var i = 0; i < services.Count; i++)
        {
            if (services[i] is { ServiceType: { } t, ImplementationInstance: DocSiteOptions existing } && t == typeof(DocSiteOptions))
            {
                if (Array.IndexOf(existing.AdditionalRoutingAssemblies, asm) >= 0) return;

                var updated = existing with
                {
                    AdditionalRoutingAssemblies = [.. existing.AdditionalRoutingAssemblies, asm],
                };
                services[i] = ServiceDescriptor.Singleton(updated);
                return;
            }
        }

        throw new InvalidOperationException(
            "AddApiReference requires AddDocSite to be called first so the API-reference assembly can be registered for Blazor routing.");
    }
}

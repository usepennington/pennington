namespace Pennington.DocSite.Api;

using Components.Reference;
using Mdazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pennington.ApiMetadata;
using Pennington.Content;

/// <summary>
/// DI extension methods for registering the API reference package.
/// </summary>
public static class ApiReferenceServiceExtensions
{
    /// <summary>
    /// Registers one named API-reference tree. Call once per library you want to
    /// document. Each call pairs with a matching <c>AddApiMetadataFrom*(name, …)</c>
    /// provider registration and publishes its type pages at the configured
    /// <see cref="ApiReferenceRegistrationOptions.RoutePrefix"/>.
    /// </summary>
    /// <remarks>
    /// Call after <see cref="DocSiteServiceExtensions.AddDocSite"/>.
    /// Shared services (Mdazor components, routing-assembly hook, registry singleton)
    /// are installed once regardless of how many times this extension is called.
    /// </remarks>
    /// <param name="services">Service collection.</param>
    /// <param name="name">Registration name. Must match the name used in the provider extension. Defaults to <c>"default"</c>.</param>
    /// <param name="configure">Optional options configuration; leave null to take all defaults (<c>/reference/api/</c> prefix).</param>
    public static IServiceCollection AddApiReference(
        this IServiceCollection services,
        string name = "default",
        Action<ApiReferenceRegistrationOptions>? configure = null)
    {
        var options = new ApiReferenceRegistrationOptions();
        configure?.Invoke(options);
        var prefix = NormalizePrefix(options.RoutePrefix);
        var registration = new ApiReferenceRegistration(
            name, prefix, options.TocTitle, options.TocSectionLabel, options.SearchPriority);

        services.AddSingleton(registration);
        RegisterPrefixPriority(services, prefix, options.SearchPriority);

        services.AddKeyedSingleton(name, (sp, key) =>
            ActivatorUtilities.CreateInstance<ApiReferenceIndex>(sp,
                sp.GetRequiredKeyedService<IApiMetadataProvider>(key),
                (string)key!));

        services.AddSingleton<IContentService>(sp =>
            new ApiReferenceContentService(
                sp.GetRequiredKeyedService<ApiReferenceIndex>(name),
                registration));

        RegisterSharedOnce(services);
        return services;
    }

    // Stamp the API tree's route prefix onto the shared search options so SearchIndexBuilder gives
    // every page under it this priority — dropping generated reference pages below comparable prose.
    // SearchIndexOptions is registered as a singleton instance by AddPennington (via AddDocSite),
    // so it is already in the collection at this point; the mutation lands before any index build.
    private static void RegisterPrefixPriority(IServiceCollection services, string prefix, int priority)
    {
        for (var i = 0; i < services.Count; i++)
        {
            if (services[i] is { ServiceType: { } t, ImplementationInstance: Pennington.Search.SearchIndexOptions search }
                && t == typeof(Pennington.Search.SearchIndexOptions))
            {
                search.PrefixPriorities[prefix] = priority;
                return;
            }
        }

        throw new InvalidOperationException(
            "AddApiReference requires AddDocSite to be called first so the API-reference route prefix "
            + "can be registered as a search priority.");
    }

    private static string NormalizePrefix(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("RoutePrefix must be non-empty.", nameof(raw));
        }
        var trimmed = raw.Trim('/');
        return trimmed.Length == 0 ? "/" : "/" + trimmed + "/";
    }

    private static void RegisterSharedOnce(IServiceCollection services)
    {
        services.TryAddSingleton<ApiReferenceRegistrationRegistry>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            Microsoft.AspNetCore.Hosting.IStartupFilter,
            ApiReferenceStartupFilter>());

        // Idempotent Mdazor component registrations — AddMdazorComponent is safe
        // to call repeatedly for the same component type, but we still guard so
        // the routing-assembly hook runs exactly once.
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
    }

    private static void AppendRoutingAssembly(IServiceCollection services)
    {
        var asm = typeof(ApiReferenceServiceExtensions).Assembly;

        for (var i = 0; i < services.Count; i++)
        {
            if (services[i] is { ServiceType: { } t, ImplementationInstance: DocSiteOptions existing } && t == typeof(DocSiteOptions))
            {
                if (Array.IndexOf(existing.AdditionalRoutingAssemblies, asm) >= 0)
                {
                    return;
                }

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
namespace Pennington.DocSite.Api.Components.Reference;

using Microsoft.Extensions.DependencyInjection;
using Pennington.ApiMetadata;

/// <summary>Small helper that picks the correct keyed <see cref="IApiMetadataProvider"/> for an API-reference component render. Priority: explicit component parameter &gt; cascading value from the enclosing page &gt; <c>"default"</c>.</summary>
internal static class ApiSourceResolver
{
    public static string ResolveName(string? explicitSource, string? cascadingSource)
        => explicitSource ?? cascadingSource ?? "default";

    public static IApiMetadataProvider ResolveProvider(
        IServiceProvider services, string? explicitSource, string? cascadingSource)
        => services.GetRequiredKeyedService<IApiMetadataProvider>(ResolveName(explicitSource, cascadingSource));
}
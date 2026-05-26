namespace Pennington.LlmsTxt;

using Microsoft.Extensions.DependencyInjection;

/// <summary>DI extension methods for the llms.txt feature.</summary>
public static class LlmsTxtServiceExtensions
{
    /// <summary>
    /// Registers a <see cref="LlmsSubtree"/> so all leaves under <see cref="LlmsSubtree.RoutePrefix"/>
    /// get split out into a dedicated <c>{RoutePrefix}llms.txt</c>. Multiple registrations are allowed;
    /// programmatic registrations override <c>_meta.yml</c>-discovered subtrees with the same prefix.
    /// </summary>
    public static IServiceCollection AddLlmsSubtree(this IServiceCollection services, LlmsSubtree subtree)
    {
        services.AddSingleton(subtree);
        return services;
    }
}
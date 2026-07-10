using Microsoft.Extensions.DependencyInjection;
using Pennington.Head;
using Pennington.Markdown.Extensions;

namespace Pennington.Beck;

/// <summary>Dependency injection extensions for registering the Pennington Beck integration.</summary>
public static class BeckServiceExtensions
{
    /// <summary>
    /// Adds build-time Beck diagram rendering — <c>```beck</c> fences (and the
    /// <c>```beck:symbol</c> file-embed form) render to self-animating inline SVG through the
    /// pure-C# Beck engine, no client-side rendering. Configure fonts, an exact text measurer, or
    /// a site-wide default style via <see cref="BeckOptions.RenderOptions"/>. Each embed gets a
    /// fullscreen-zoom button backed by a small head-contributed script; opt out with
    /// <see cref="BeckOptions.Zoom"/>.
    /// </summary>
    public static IServiceCollection AddPenningtonBeck(this IServiceCollection services, Action<BeckOptions>? configure = null)
    {
        var options = new BeckOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<ICodeBlockPreprocessor, BeckCodeBlockPreprocessor>();
        services.AddHeadContributor<BeckZoomHeadContributor>();
        return services;
    }
}

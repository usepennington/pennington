namespace Pennington.TreeSitter;

using Fragments;
using Microsoft.Extensions.DependencyInjection;
using Parsing;
using Pennington.Infrastructure;
using Pennington.Markdown.Extensions;
using Preprocessing;
using Resolution;

/// <summary>Dependency injection extensions for registering the Pennington tree-sitter integration.</summary>
public static class TreeSitterExtensions
{
    /// <summary>
    /// Adds tree-sitter based multi-language code-fragment extraction — the <c>:symbol</c> fence modifier.
    /// Services are registered only when <see cref="TreeSitterOptions.ContentRoot"/> is configured.
    /// </summary>
    public static IServiceCollection AddPenningtonTreeSitter(this IServiceCollection services, Action<TreeSitterOptions>? configure = null)
    {
        var options = new TreeSitterOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        if (!string.IsNullOrEmpty(options.ContentRoot))
        {
            services.AddSingleton<ITreeSitterParserPool, TreeSitterParserPool>();
            services.AddSingleton<NamePathResolver>();
            services.AddTransient<ISourceFragmentService, SourceFragmentService>();
            services.AddSingleton<ICodeBlockPreprocessor, TreeSitterCodeBlockPreprocessor>();

            // Watch the source directory so editing a referenced file triggers live-reload.
            // The shared FileWatcher fan-out reaches LiveReloadServer — no explicit reload call needed.
            services.AddSingleton<IFileWatchAware, TreeSitterContentWatcher>();
        }

        return services;
    }
}

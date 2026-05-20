namespace Pennington.Roslyn;

using Highlighting;
using Markdown.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Pennington.ApiMetadata;
using Pennington.Highlighting;
using Preprocessing;
using Symbols;
using Workspace;

/// <summary>Dependency injection extensions for registering the Pennington Roslyn integration.</summary>
public static class RoslynExtensions
{
    /// <summary>Add Roslyn-based code analysis and highlighting.</summary>
    public static IServiceCollection AddPenningtonRoslyn(this IServiceCollection services, Action<RoslynOptions>? configure = null)
    {
        var options = new RoslynOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Always register basic highlighter (works without solution)
        services.AddSingleton<SyntaxHighlighter>();
        services.AddSingleton<ICodeHighlighter, RoslynHighlighter>();

        // If solution path configured, register workspace + symbols + preprocessor
        if (!string.IsNullOrEmpty(options.SolutionPath))
        {
            services.AddSingleton<ISolutionWorkspaceService, SolutionWorkspaceService>();
            services.AddSingleton<ISymbolExtractionService>(sp =>
            {
                var symbolService = ActivatorUtilities.CreateInstance<SymbolExtractionService>(sp);
                if (sp.GetRequiredService<ISolutionWorkspaceService>() is SolutionWorkspaceService sws)
                {
                    sws.SymbolExtractionService = symbolService;
                }

                return symbolService;
            });
            services.AddSingleton<ICodeBlockPreprocessor, RoslynCodeBlockPreprocessor>();

            services.AddSingleton<IXmlDocParser, XmlDocParser>();
            services.AddSingleton<IXmlDocHtmlRenderer, XmlDocHtmlRenderer>();

            services.AddHostedService<SymbolExtractionWarmupService>();
        }

        return services;
    }
}
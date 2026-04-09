namespace Pennington.Roslyn;

using Microsoft.Extensions.DependencyInjection;
using Pennington.Highlighting;
using Pennington.Markdown.Extensions;
using Pennington.Roslyn.Highlighting;
using Pennington.Roslyn.Preprocessing;
using Pennington.Roslyn.Symbols;
using Pennington.Roslyn.Workspace;

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
        }

        return services;
    }
}

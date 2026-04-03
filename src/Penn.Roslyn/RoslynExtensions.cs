namespace Penn.Roslyn;

using Microsoft.Extensions.DependencyInjection;
using Penn.Highlighting;
using Penn.Markdown.Extensions;
using Penn.Roslyn.Highlighting;
using Penn.Roslyn.Preprocessing;
using Penn.Roslyn.Symbols;
using Penn.Roslyn.Workspace;

public static class RoslynExtensions
{
    /// <summary>Add Roslyn-based code analysis and highlighting.</summary>
    public static IServiceCollection AddPennRoslyn(this IServiceCollection services, Action<RoslynOptions>? configure = null)
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
            services.AddSingleton<ISymbolExtractionService, SymbolExtractionService>();
            services.AddSingleton<ICodeBlockPreprocessor, RoslynCodeBlockPreprocessor>();
        }

        return services;
    }
}

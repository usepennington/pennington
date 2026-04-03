namespace Penn.Roslyn;

using Microsoft.Extensions.DependencyInjection;
using Penn.Highlighting;
using Penn.Roslyn.Highlighting;

public static class RoslynExtensions
{
    /// <summary>Add Roslyn-based code analysis and highlighting.</summary>
    public static IServiceCollection AddPennRoslyn(this IServiceCollection services, Action<RoslynOptions>? configure = null)
    {
        var options = new RoslynOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Always register the Roslyn highlighter (priority 100, AdhocWorkspace-based)
        services.AddSingleton<ICodeHighlighter, RoslynHighlighter>();

        // Workspace + preprocessor registration will be added in later tasks
        // when SolutionPath is configured

        return services;
    }
}

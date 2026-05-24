namespace Pennington.Roslyn;

/// <summary>Configuration for Roslyn integration.</summary>
public sealed class RoslynOptions
{
    /// <summary>Path to a .sln, .slnx, or .slnf file. If null, only basic highlighting is enabled.</summary>
    public string? SolutionPath { get; set; }

    /// <summary>Optional project filter.</summary>
    public ProjectFilter? ProjectFilter { get; set; }

    /// <summary>When true (the default), registers the <c>:xmldocid</c>/<c>:path</c> code-block preprocessor. Set false to keep the workspace and API-metadata services for reflection while delegating code-fragment fences to another preprocessor (for example tree-sitter <c>:symbol</c>).</summary>
    public bool EnableCodeFragmentFences { get; set; } = true;
}

/// <summary>Filter for which projects to analyze.</summary>
public record ProjectFilter
{
    /// <summary>Project names to include; when non-null, only these projects are analyzed.</summary>
    public HashSet<string>? IncludedProjects { get; init; }
    /// <summary>Project names to exclude from analysis.</summary>
    public HashSet<string>? ExcludedProjects { get; init; }
}
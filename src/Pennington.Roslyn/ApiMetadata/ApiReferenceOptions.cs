namespace Pennington.Roslyn.ApiMetadata;

using System.Reflection;
using Microsoft.CodeAnalysis;

/// <summary>Filters applied by <see cref="RoslynApiMetadataProvider"/> when walking the Roslyn workspace.</summary>
public sealed record ApiReferenceOptions
{
    /// <summary>Predicate selecting which projects contribute types. Defaults to <see cref="DefaultProjectFilter"/> when unset.</summary>
    public Predicate<Project>? ProjectFilter { get; set; }

    /// <summary>Extra per-type inclusion predicate applied on top of the built-in rules (public, non-delegate, non-attribute, non-<c>ComponentBase</c>, has xmldoc).</summary>
    public Predicate<INamedTypeSymbol>? TypeFilter { get; set; }

    /// <summary>Built-in project filter that excludes <c>*.Tests</c> / <c>*.IntegrationTests</c> and the entry assembly.</summary>
    public static Predicate<Project> DefaultProjectFilter()
    {
        var entryName = Assembly.GetEntryAssembly()?.GetName().Name;
        return project =>
        {
            var name = StripTargetFrameworkSuffix(project.Name);
            if (name.EndsWith(".Tests", StringComparison.Ordinal))
            {
                return false;
            }

            if (name.EndsWith(".IntegrationTests", StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(entryName) && name == entryName)
            {
                return false;
            }

            return true;
        };
    }

    private static string StripTargetFrameworkSuffix(string name)
    {
        var open = name.LastIndexOf('(');
        return open >= 0 && name.EndsWith(')') ? name[..open] : name;
    }
}
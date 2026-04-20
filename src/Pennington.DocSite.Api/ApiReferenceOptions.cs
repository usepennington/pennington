namespace Pennington.DocSite.Api;

using System.Reflection;
using Microsoft.CodeAnalysis;

/// <summary>
/// Options controlling which projects and types the API reference package surfaces.
/// </summary>
public sealed record ApiReferenceOptions
{
    /// <summary>
    /// Predicate selecting which projects in the Roslyn workspace contribute types.
    /// When null, the default filter excludes <c>*.Tests</c> / <c>*.IntegrationTests</c>
    /// projects and the entry assembly's own project (so a doc site doesn't publish
    /// reference pages for itself).
    /// </summary>
    public Predicate<Project>? ProjectFilter { get; set; }

    /// <summary>
    /// Additional per-type inclusion predicate applied on top of the built-in
    /// rules (public, non-delegate, non-attribute, non-<c>ComponentBase</c>, has xmldoc).
    /// When null, no extra filtering is applied.
    /// </summary>
    public Predicate<INamedTypeSymbol>? TypeFilter { get; set; }

    /// <summary>
    /// The built-in project filter applied when <see cref="ProjectFilter"/> is null:
    /// excludes <c>*.Tests</c> / <c>*.IntegrationTests</c> projects and the entry
    /// assembly. Returned as a delegate so consumers can compose it with extra
    /// constraints (e.g. AND-ing a <c>Name.StartsWith("MyLibrary")</c> check).
    /// </summary>
    public static Predicate<Project> DefaultProjectFilter()
    {
        var entryName = Assembly.GetEntryAssembly()?.GetName().Name;
        return project =>
        {
            var name = StripTargetFrameworkSuffix(project.Name);
            if (name.EndsWith(".Tests", StringComparison.Ordinal)) return false;
            if (name.EndsWith(".IntegrationTests", StringComparison.Ordinal)) return false;
            if (!string.IsNullOrEmpty(entryName) && name == entryName) return false;
            return true;
        };
    }

    private static string StripTargetFrameworkSuffix(string name)
    {
        var open = name.LastIndexOf('(');
        return open >= 0 && name.EndsWith(')') ? name[..open] : name;
    }
}

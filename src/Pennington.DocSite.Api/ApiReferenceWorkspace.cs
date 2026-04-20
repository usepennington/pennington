namespace Pennington.DocSite.Api;

using Microsoft.CodeAnalysis;
using Pennington.Roslyn.Workspace;

/// <summary>
/// Workspace helpers used by the API reference indexes to enumerate the
/// configured projects and their public types.
/// </summary>
public static class ApiReferenceWorkspace
{
    /// <summary>
    /// Returns every project in the solution that passes <paramref name="filter"/>.
    /// When <paramref name="filter"/> is null, the default filter from
    /// <see cref="ApiReferenceOptions.DefaultProjectFilter"/> is used.
    /// </summary>
    public static Task<IEnumerable<Project>> GetFilteredProjectsAsync(
        ISolutionWorkspaceService workspace,
        Predicate<Project>? filter)
    {
        var effective = filter ?? ApiReferenceOptions.DefaultProjectFilter();
        return workspace.GetProjectsAsync(p => effective(p));
    }

    /// <summary>
    /// Walks every type (optionally including nested types) under
    /// <paramref name="root"/>.
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> EnumerateTypes(
        INamespaceSymbol root,
        bool includeNested = false)
    {
        var queue = new Queue<INamespaceOrTypeSymbol>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            foreach (var member in queue.Dequeue().GetMembers())
            {
                if (member is INamespaceSymbol ns)
                {
                    queue.Enqueue(ns);
                }
                else if (member is INamedTypeSymbol type)
                {
                    yield return type;
                    if (includeNested)
                    {
                        foreach (var nested in type.GetTypeMembers())
                        {
                            yield return nested;
                        }
                    }
                }
            }
        }
    }
}

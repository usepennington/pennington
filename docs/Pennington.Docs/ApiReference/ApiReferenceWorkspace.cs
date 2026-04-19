namespace Pennington.Docs.ApiReference;

using Microsoft.CodeAnalysis;
using Pennington.Roslyn.Workspace;

internal static class ApiReferenceWorkspace
{
    public static Task<IEnumerable<Project>> GetPenningtonProjectsAsync(ISolutionWorkspaceService workspace)
        => workspace.GetProjectsAsync(IsPenningtonSourceProject);

    private static bool IsPenningtonSourceProject(Project project)
    {
        var name = StripTargetFrameworkSuffix(project.Name);
        return name.StartsWith("Pennington", StringComparison.Ordinal)
            && !name.EndsWith(".Tests", StringComparison.Ordinal)
            && !name.EndsWith(".IntegrationTests", StringComparison.Ordinal)
            && name != "Pennington.Docs";
    }

    private static string StripTargetFrameworkSuffix(string name)
    {
        var open = name.LastIndexOf('(');
        return open >= 0 && name.EndsWith(')') ? name[..open] : name;
    }

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

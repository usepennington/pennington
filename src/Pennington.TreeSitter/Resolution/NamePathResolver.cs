namespace Pennington.TreeSitter.Resolution;

using TsNode = global::TreeSitter.Node;

/// <summary>Resolves a dotted member path to a declaration node via a generic, config-driven tree descent.</summary>
public sealed class NamePathResolver
{
    /// <summary>
    /// Resolves <paramref name="segments"/> (e.g. <c>["Calculator", "add"]</c>) to a declaration node under
    /// <paramref name="root"/>, or null if no path matches. Each segment must name a declaration nested
    /// (through transparent wrapper nodes) within a node matched by the previous segment.
    /// </summary>
    public TsNode? Resolve(TsNode root, IReadOnlyList<string> segments, LanguageDeclarationConfig config)
    {
        if (segments.Count == 0)
        {
            return null;
        }

        // A frontier rather than a single node so that multiple declarations sharing a name
        // (C# partial classes, Rust struct + its impl blocks) are all searched for the next segment.
        var frontier = new List<TsNode> { root };
        foreach (var segment in segments)
        {
            var next = new List<TsNode>();
            foreach (var node in frontier)
            {
                foreach (var child in TreeWalker.ChildrenMatching(node, config.TransparentNodeTypes, config.DeclarationNodeTypes))
                {
                    if (EffectiveName(child, config) == segment)
                    {
                        next.Add(child);
                    }
                }
            }

            if (next.Count == 0)
            {
                return null;
            }

            frontier = next;
        }

        return frontier[0];
    }

    private static string? EffectiveName(TsNode node, LanguageDeclarationConfig config) =>
        node.GetChildForField(config.NameFieldFor(node.Type))?.Text;
}

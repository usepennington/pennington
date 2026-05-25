namespace Pennington.TreeSitter.Resolution;

using TsNode = global::TreeSitter.Node;

/// <summary>Shared config-driven descent over a syntax tree's named children.</summary>
internal static class TreeWalker
{
    /// <summary>
    /// Yields named children of <paramref name="parent"/> whose type is in <paramref name="match"/>, descending
    /// transparently through wrapper node types in <paramref name="transparent"/> so container nodes do not hide
    /// the declarations beneath them.
    /// </summary>
    public static IEnumerable<TsNode> ChildrenMatching(
        TsNode parent, IReadOnlySet<string> transparent, IReadOnlySet<string> match)
    {
        foreach (var child in parent.NamedChildren)
        {
            if (transparent.Contains(child.Type))
            {
                foreach (var descendant in ChildrenMatching(child, transparent, match))
                {
                    yield return descendant;
                }
            }
            else if (match.Contains(child.Type))
            {
                yield return child;
            }
        }
    }
}

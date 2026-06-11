namespace Pennington.Cli;

/// <summary>Renders a hierarchy as an ASCII tree (<c>├─ └─ │</c>) to a <see cref="TextWriter"/>.</summary>
public static class AsciiTreeWriter
{
    private const string Tee = "├─ ";
    private const string Ell = "└─ ";
    private const string Bar = "│  ";
    private const string Gap = "   ";

    /// <summary>
    /// Writes <paramref name="nodes"/> as a tree. <paramref name="label"/> formats one node to a
    /// single line; <paramref name="children"/> yields a node's children. Recurses until
    /// <paramref name="maxDepth"/> (1-based; the top level is depth 1).
    /// </summary>
    public static void Write<T>(
        TextWriter writer,
        IReadOnlyList<T> nodes,
        Func<T, string> label,
        Func<T, IReadOnlyList<T>> children,
        int maxDepth = int.MaxValue)
        => WriteLevel(writer, nodes, label, children, prefix: "", depth: 1, maxDepth);

    private static void WriteLevel<T>(
        TextWriter writer,
        IReadOnlyList<T> nodes,
        Func<T, string> label,
        Func<T, IReadOnlyList<T>> children,
        string prefix,
        int depth,
        int maxDepth)
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var isLast = i == nodes.Count - 1;
            writer.WriteLine(prefix + (isLast ? Ell : Tee) + label(nodes[i]));

            if (depth >= maxDepth)
            {
                continue;
            }

            var kids = children(nodes[i]);
            if (kids.Count > 0)
            {
                WriteLevel(writer, kids, label, children, prefix + (isLast ? Gap : Bar), depth + 1, maxDepth);
            }
        }
    }
}

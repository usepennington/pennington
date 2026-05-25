namespace Pennington.TreeSitter.Fragments;

using Resolution;
using TsNode = global::TreeSitter.Node;

/// <summary>Collects a file's top-of-file import/using/require statements for prepending to a fragment.</summary>
internal static class ImportCollector
{
    /// <summary>
    /// Returns the source text of every import node under <paramref name="root"/> in document order, joined by
    /// newlines, or an empty string when the language declares no import node types or the file has none.
    /// </summary>
    public static string Collect(TsNode root, LanguageDeclarationConfig config)
    {
        if (config.ImportNodeTypes.Count == 0)
        {
            return string.Empty;
        }

        var imports = TreeWalker
            .ChildrenMatching(root, config.TransparentNodeTypes, config.ImportNodeTypes)
            .Select(node => node.Text.Replace("\r\n", "\n"));

        return string.Join("\n", imports);
    }
}

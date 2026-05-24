namespace Pennington.TreeSitter.Resolution;

/// <summary>
/// Describes, for one language, which syntax-tree node types are named declarations and how to read a
/// declaration's name, so a single generic walker can resolve a dotted member path across languages.
/// </summary>
public sealed record LanguageDeclarationConfig
{
    /// <summary>Language identifier passed to the tree-sitter binding (e.g. <c>C#</c>, <c>Python</c>, <c>Rust</c>).</summary>
    public required string TreeSitterLanguageName { get; init; }

    /// <summary>Node types that count as named declarations (e.g. <c>class_declaration</c>, <c>method_declaration</c>).</summary>
    public required IReadOnlySet<string> DeclarationNodeTypes { get; init; }

    /// <summary>Field name holding a declaration's name node. Defaults to <c>name</c>.</summary>
    public string DefaultNameField { get; init; } = "name";

    /// <summary>Per-node-type overrides for the name field (e.g. Rust <c>impl_item</c> is identified by its <c>type</c> field).</summary>
    public IReadOnlyDictionary<string, string> NameFieldOverrides { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Structural wrapper node types the walker descends through transparently (e.g. C# <c>declaration_list</c>,
    /// Python <c>block</c>), so name matching is not tripped up by grammar container nodes.
    /// </summary>
    public IReadOnlySet<string> TransparentNodeTypes { get; init; } = new HashSet<string>();

    /// <summary>Field name holding a declaration's body, used for body-only extraction. Defaults to <c>body</c>.</summary>
    public string BodyFieldName { get; init; } = "body";

    /// <summary>Returns the field name to read for the given declaration node type's name.</summary>
    public string NameFieldFor(string nodeType) =>
        NameFieldOverrides.TryGetValue(nodeType, out var field) ? field : DefaultNameField;
}

namespace Pennington.TreeSitter.Fragments;

/// <summary>Per-reference extraction flags parsed from a <c>:symbol</c> info-string.</summary>
public sealed record FragmentOptions
{
    /// <summary>Emit only the declaration's body, stripping the signature and enclosing braces.</summary>
    public bool BodyOnly { get; init; }

    /// <summary>Prepend the file's top-of-file import/using/require statements to the fragment.</summary>
    public bool IncludeImports { get; init; }

    /// <summary>Render the node with member bodies replaced by an elision marker (outline view).</summary>
    public bool SignaturesOnly { get; init; }

    /// <summary>The default: full declaration text, no imports, no elision.</summary>
    public static FragmentOptions Default { get; } = new();
}

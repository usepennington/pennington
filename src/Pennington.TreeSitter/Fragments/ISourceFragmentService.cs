namespace Pennington.TreeSitter.Fragments;

/// <summary>Resolves a source file plus an optional member name path to its source text using tree-sitter.</summary>
public interface ISourceFragmentService
{
    /// <summary>
    /// Returns the source text of <paramref name="namePath"/> within <paramref name="relativeFilePath"/>, or a
    /// failure. An empty <paramref name="namePath"/> returns the whole file. <paramref name="options"/> select
    /// body-only extraction, an elided-body outline, and whether the file's imports are prepended.
    /// </summary>
    FragmentResult GetFragment(string languageId, string relativeFilePath, string namePath, FragmentOptions options);
}

/// <summary>Outcome of a fragment lookup: either the extracted <see cref="Text"/> or an <see cref="Error"/> message.</summary>
/// <param name="Text">The extracted source text when successful; otherwise null.</param>
/// <param name="Error">The failure message when unsuccessful; otherwise null.</param>
public sealed record FragmentResult(string? Text, string? Error)
{
    /// <summary>True when extraction succeeded.</summary>
    public bool Succeeded => Error is null;

    /// <summary>Creates a successful result wrapping <paramref name="text"/>.</summary>
    public static FragmentResult Ok(string text) => new(text, null);

    /// <summary>Creates a failed result with the given <paramref name="error"/> message.</summary>
    public static FragmentResult Fail(string error) => new(null, error);
}

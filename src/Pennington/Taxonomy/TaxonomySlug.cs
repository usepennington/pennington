namespace Pennington.Taxonomy;

using System.Text.RegularExpressions;

/// <summary>
/// The default taxonomy slug encoding, shared so links that point at a term page (e.g. tag chips on a
/// post) produce the same URL that <see cref="TaxonomyContentService{TFrontMatter, TKey}"/> discovers.
/// </summary>
public static partial class TaxonomySlug
{
    /// <summary>
    /// Lowercases <paramref name="value"/>, collapses whitespace runs to single hyphens, and
    /// URL-encodes any remaining unsafe characters.
    /// </summary>
    public static string Slugify(string value)
    {
        var raw = (value ?? "").Trim().ToLowerInvariant();
        var hyphenated = Whitespace().Replace(raw, "-");
        return Uri.EscapeDataString(hyphenated);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}

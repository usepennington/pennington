namespace Pennington.Taxonomy;

using FrontMatter;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Configures one taxonomy axis (browse-by-cuisine, browse-by-tag, browse-by-audience, ...).
/// Pass to <see cref="TaxonomyServiceExtensions.AddTaxonomy{TFrontMatter, TKey}"/>; the
/// extension validates the options and registers a <see cref="TaxonomyContentService{TFrontMatter, TKey}"/>
/// against them.
/// </summary>
/// <typeparam name="TFrontMatter">
/// The front-matter type used to parse source markdown items. Items whose source is not a
/// <see cref="Pipeline.MarkdownFileSource"/> are ignored.
/// </typeparam>
/// <typeparam name="TKey">
/// The taxonomy key type. Most sites use <see cref="string"/>; <see cref="Enum"/> or
/// numeric keys also work as long as they support equality.
/// </typeparam>
public sealed class TaxonomyOptions<TFrontMatter, TKey>
    where TFrontMatter : IFrontMatter, new()
    where TKey : notnull
{
    /// <summary>
    /// Base URL the taxonomy mounts under. The index lives at this URL; per-term pages
    /// at <c>{BaseUrl}/{slug}/</c>. Required.
    /// </summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>
    /// Single-valued projection. Set this OR <see cref="SelectKeys"/> — exactly one is required.
    /// Items whose projection returns <c>null</c> (when <typeparamref name="TKey"/> is nullable)
    /// or the default value are skipped.
    /// </summary>
    public Func<TFrontMatter, TKey?>? SelectKey { get; set; }

    /// <summary>
    /// Multi-valued projection. Set this OR <see cref="SelectKey"/> — exactly one is required.
    /// Each returned key produces a (key, item) pair. Empty enumerations cause the item to
    /// be skipped.
    /// </summary>
    public Func<TFrontMatter, IEnumerable<TKey>>? SelectKeys { get; set; }

    /// <summary>
    /// Razor component that renders the index at <see cref="BaseUrl"/>. Receives the full
    /// term list as a <c>Terms</c> parameter. Required.
    /// </summary>
    public Type? IndexPage { get; set; }

    /// <summary>
    /// Razor component that renders each per-term page at <c>{BaseUrl}/{slug}/</c>. Receives
    /// the matching <see cref="TaxonomyTerm{TFrontMatter, TKey}"/> as a <c>Term</c> parameter. Required.
    /// </summary>
    public Type? TermPage { get; set; }

    /// <summary>
    /// Slug encoder. Default lowercases the key's string form and replaces whitespace with
    /// hyphens; URL-encodes any remaining unsafe characters.
    /// </summary>
    public Func<TKey, string> SlugFor { get; set; } = DefaultSlug;

    /// <summary>Optional human-readable label override. Default returns the key's string form.</summary>
    public Func<TKey, string> LabelFor { get; set; } = key => key.ToString() ?? "";

    /// <summary>
    /// Section label applied to the navigation entries. Defaults to the last segment of
    /// <see cref="BaseUrl"/> with the first letter uppercased.
    /// </summary>
    public string? SectionLabel { get; set; }

    /// <summary>Search-index priority for the discovered routes. Higher ranks first. Default <c>10</c>.</summary>
    public int SearchPriority { get; set; } = 10;

    /// <summary>Whether to publish a cross-reference per term (e.g. <c>tag-csharp</c>). Default <c>true</c>.</summary>
    public bool EmitCrossReferences { get; set; } = true;

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new InvalidOperationException($"{nameof(TaxonomyOptions<TFrontMatter, TKey>)}.{nameof(BaseUrl)} is required.");
        }

        if (!BaseUrl.StartsWith('/'))
        {
            throw new InvalidOperationException($"{nameof(BaseUrl)} '{BaseUrl}' must start with '/'.");
        }

        if (SelectKey is null == SelectKeys is null)
        {
            throw new InvalidOperationException(
                $"Set exactly one of {nameof(SelectKey)} (single-valued) or {nameof(SelectKeys)} (multi-valued) on {nameof(TaxonomyOptions<TFrontMatter, TKey>)}.");
        }

        if (IndexPage is null)
        {
            throw new InvalidOperationException($"{nameof(IndexPage)} is required (the Razor component for the {BaseUrl} index).");
        }

        if (TermPage is null)
        {
            throw new InvalidOperationException($"{nameof(TermPage)} is required (the Razor component for {BaseUrl}/{{slug}} pages).");
        }

        if (!typeof(IComponent).IsAssignableFrom(IndexPage))
        {
            throw new InvalidOperationException($"{nameof(IndexPage)} ({IndexPage.FullName}) must implement {nameof(IComponent)}.");
        }

        if (!typeof(IComponent).IsAssignableFrom(TermPage))
        {
            throw new InvalidOperationException($"{nameof(TermPage)} ({TermPage.FullName}) must implement {nameof(IComponent)}.");
        }
    }

    internal string ResolvedSectionLabel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(SectionLabel))
            {
                return SectionLabel!;
            }

            var segment = BaseUrl.TrimEnd('/').Split('/').Last();
            if (string.IsNullOrEmpty(segment))
            {
                return "Taxonomy";
            }

            return char.ToUpperInvariant(segment[0]) + segment[1..];
        }
    }

    internal string IndexUrl => "/" + BaseUrl.Trim('/') + "/";

    internal string TermUrl(string slug) => IndexUrl + slug + "/";

    private static string DefaultSlug(TKey key)
    {
        var raw = (key.ToString() ?? "").Trim().ToLowerInvariant();
        var hyphenated = System.Text.RegularExpressions.Regex.Replace(raw, @"\s+", "-");
        return Uri.EscapeDataString(hyphenated);
    }
}
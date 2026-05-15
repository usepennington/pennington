namespace Pennington.Taxonomy;

using System.Collections.Immutable;
using FrontMatter;
using Routing;

/// <summary>
/// One value of the taxonomy axis along with every content item that projects to it.
/// Razor pages registered with <see cref="TaxonomyOptions{TFrontMatter, TKey}.TermPage"/>
/// receive an instance of this type as their <c>Term</c> parameter; index pages receive
/// the full <see cref="IReadOnlyList{T}"/> as their <c>Terms</c> parameter.
/// </summary>
/// <typeparam name="TFrontMatter">The front-matter record the source items were parsed as.</typeparam>
/// <typeparam name="TKey">The taxonomy key type (typically <see cref="string"/>).</typeparam>
/// <param name="Key">The raw key value (the input to <see cref="TaxonomyOptions{TFrontMatter, TKey}.SlugFor"/>).</param>
/// <param name="Label">Human-readable label. Defaults to the key's string form; override via <see cref="TaxonomyOptions{TFrontMatter, TKey}.LabelFor"/>.</param>
/// <param name="Slug">URL-safe slug; the last segment of <see cref="Url"/>.</param>
/// <param name="Url">Absolute URL of this term page (e.g. <c>/cuisine/italian/</c>).</param>
/// <param name="Items">Every content item that projected to this key, in the order they were discovered.</param>
public record TaxonomyTerm<TFrontMatter, TKey>(
    TKey Key,
    string Label,
    string Slug,
    UrlPath Url,
    ImmutableList<TaxonomyItem<TFrontMatter>> Items)
    where TFrontMatter : IFrontMatter;

/// <summary>
/// One content item discovered by the taxonomy walker, paired with the URL of the page that
/// owns it. Surfaces the parsed front matter so term pages can render titles, dates, summaries,
/// and any other field the consumer needs without re-reading the source file.
/// </summary>
/// <typeparam name="TFrontMatter">The front-matter record the source item was parsed as.</typeparam>
/// <param name="FrontMatter">Parsed front matter from the source markdown file.</param>
/// <param name="Url">URL of the page the front matter belongs to.</param>
public record TaxonomyItem<TFrontMatter>(
    TFrontMatter FrontMatter,
    UrlPath Url)
    where TFrontMatter : IFrontMatter;

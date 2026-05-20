namespace Pennington.Taxonomy;

/// <summary>
/// Marker interface for every closed-generic <see cref="TaxonomyContentService{TFrontMatter, TKey}"/>.
/// Lets a taxonomy service identify other taxonomies in the registered <see cref="Content.IContentService"/>
/// set without reflecting on the open generic — used to break the otherwise-circular dependency
/// when two taxonomies sit on the same content tree.
/// </summary>
public interface ITaxonomyContentService
{
}
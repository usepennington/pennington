namespace Pennington.StructuredData;

/// <summary>
/// Capability interface for an <see cref="FrontMatter.IFrontMatter"/> type that
/// can describe its own schema.org JSON-LD. Templates that render content
/// (DocSite, BlogSite, custom hosts) check for this capability and emit the
/// returned entities through <c>&lt;StructuredData&gt;</c>.
/// </summary>
/// <remarks>
/// This is a capability mixin — implement it alongside <see cref="FrontMatter.IFrontMatter"/>
/// the same way you would <see cref="FrontMatter.ITaggable"/> or
/// <see cref="FrontMatter.IOrderable"/>. The framework does not auto-wire it;
/// the rendering template is responsible for calling
/// <see cref="GetStructuredData"/> and including its output.
/// </remarks>
public interface IHasStructuredData
{
    /// <summary>
    /// Returns the schema.org entities to emit on the page. Implementations
    /// typically yield one <c>Article</c>/<c>Recipe</c>/<c>Product</c>/etc.
    /// built from front matter values, plus the
    /// <see cref="StructuredDataContext.CanonicalUrl"/> the template supplies.
    /// </summary>
    IEnumerable<JsonLdEntity> GetStructuredData(StructuredDataContext context);
}

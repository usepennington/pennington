namespace Pennington.FrontMatter;

/// <summary>
/// Content that supports tags.
/// </summary>
public interface ITaggable
{
    /// <summary>Tags applied to this content.</summary>
    string[] Tags { get; }
}

/// <summary>
/// Content that can redirect to another URL.
/// </summary>
public interface IRedirectable
{
    /// <summary>Target URL when this page should redirect; <c>null</c> or empty means no redirect.</summary>
    string? RedirectUrl { get; }
}

/// <summary>
/// Content that carries a section label. The label surfaces on breadcrumbs and
/// prev/next navigation; it does NOT drive sidebar grouping (the subfolder under
/// an area drives grouping — see <see cref="Navigation.NavigationBuilder"/>).
/// </summary>
public interface ISectionable
{
    /// <summary>Section label for this page, used in breadcrumbs and prev/next navigation.</summary>
    string? SectionLabel { get; }
}

/// <summary>
/// Content that has explicit ordering.
/// </summary>
public interface IOrderable
{
    /// <summary>Sort order for this page within its section (lower sorts first).</summary>
    int Order { get; }
}
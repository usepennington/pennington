namespace Pennington.FrontMatter;

/// <summary>
/// Content that supports tags.
/// </summary>
public interface ITaggable { string[] Tags { get; } }

/// <summary>
/// Content that can redirect to another URL.
/// </summary>
public interface IRedirectable { string? RedirectUrl { get; } }

/// <summary>
/// Content that carries a section label. The label surfaces on breadcrumbs and
/// prev/next navigation; it does NOT drive sidebar grouping (the subfolder under
/// an area drives grouping — see <see cref="Navigation.NavigationBuilder"/>).
/// </summary>
public interface ISectionable { string? SectionLabel { get; } }

/// <summary>
/// Content that has explicit ordering.
/// </summary>
public interface IOrderable { int Order { get; } }
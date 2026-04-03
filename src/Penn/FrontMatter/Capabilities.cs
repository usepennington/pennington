namespace Penn.FrontMatter;

/// <summary>
/// Content that can be marked as draft.
/// </summary>
public interface IDraftable { bool IsDraft { get; } }

/// <summary>
/// Content that supports tags.
/// </summary>
public interface ITaggable { string[] Tags { get; } }

/// <summary>
/// Content that can redirect to another URL.
/// </summary>
public interface IRedirectable { string? RedirectUrl { get; } }

/// <summary>
/// Content that belongs to a section.
/// </summary>
public interface ISectionable { string? Section { get; } }

/// <summary>
/// Content that can be cross-referenced by UID.
/// </summary>
public interface ICrossReferenceable { string? Uid { get; } }

/// <summary>
/// Content that has explicit ordering.
/// </summary>
public interface IOrderable { int Order { get; } }

/// <summary>
/// Content with a description.
/// </summary>
public interface IDescribable { string? Description { get; } }

/// <summary>
/// Content with a publication date.
/// </summary>
public interface IDateable { DateTime? Date { get; } }

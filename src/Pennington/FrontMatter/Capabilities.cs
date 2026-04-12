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
/// Content that belongs to a section.
/// </summary>
public interface ISectionable { string? Section { get; } }

/// <summary>
/// Content that has explicit ordering.
/// </summary>
public interface IOrderable { int Order { get; } }

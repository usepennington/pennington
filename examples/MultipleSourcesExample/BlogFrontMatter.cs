namespace MultipleSourcesExample;

using Pennington.FrontMatter;

/// <summary>
/// A second front-matter shape distinct from <see cref="DocFrontMatter"/>.
/// Demonstrates how a bare <c>AddPennington</c> host can chain two
/// <c>AddMarkdownContent&lt;T&gt;</c> registrations with unrelated types.
/// </summary>
public record BlogFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string? Uid { get; init; }
    public DateTime? PublishedOn { get; init; }
    public string? Author { get; init; }
}
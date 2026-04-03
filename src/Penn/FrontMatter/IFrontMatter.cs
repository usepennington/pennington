namespace Penn.FrontMatter;

/// <summary>
/// Minimum: every content page has a title.
/// </summary>
public interface IFrontMatter
{
    string Title { get; }
}

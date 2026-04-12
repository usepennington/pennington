namespace Pennington.FrontMatter;

/// <summary>
/// Minimum: every content page has a title.
/// Default members provide sensible opt-out values so implementations
/// only declare properties they actually parse from front matter.
/// </summary>
public interface IFrontMatter
{
    string Title { get; }
    bool IsDraft => false;
    bool Search => true;
    bool Llms => true;
    string? Uid => null;
    string? Description => null;
    DateTime? Date => null;
}

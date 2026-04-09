namespace Pennington.Pipeline;

using Pennington.FrontMatter;
using Pennington.Routing;

/// <summary>
/// Produces front matter and raw content for the Parse stage.
/// </summary>
public interface IProgrammaticContentGenerator
{
    Task<ProgrammaticContent> GenerateAsync(ContentRoute route);
}

public record TextProgrammaticContent(
    IFrontMatter? Metadata,
    string RawContent,
    string ContentType = "text/html"
);

public record BinaryProgrammaticContent(
    Func<Task<byte[]>> ByteGenerator,
    string ContentType
);

public union ProgrammaticContent(TextProgrammaticContent, BinaryProgrammaticContent);

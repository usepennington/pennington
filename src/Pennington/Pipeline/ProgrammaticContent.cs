namespace Pennington.Pipeline;

using FrontMatter;
using Routing;

/// <summary>
/// Produces front matter and raw content for the Parse stage.
/// </summary>
public interface IProgrammaticContentGenerator
{
    /// <summary>Generates the content for the given route.</summary>
    Task<ProgrammaticContent> GenerateAsync(ContentRoute route);
}

/// <summary>Programmatic content produced as a text body.</summary>
/// <param name="Metadata">Optional front matter metadata.</param>
/// <param name="RawContent">Raw body text (markdown or HTML depending on content type).</param>
/// <param name="ContentType">MIME type for the response.</param>
public record TextProgrammaticContent(
    IFrontMatter? Metadata,
    string RawContent,
    string ContentType = "text/html"
);

/// <summary>Programmatic content produced as a binary payload.</summary>
/// <param name="ByteGenerator">Deferred byte producer invoked when the content is needed.</param>
/// <param name="ContentType">MIME type for the response.</param>
public record BinaryProgrammaticContent(
    Func<Task<byte[]>> ByteGenerator,
    string ContentType
);

/// <summary>Union of supported programmatic content payload shapes.</summary>
#if NET11_0_OR_GREATER
public union ProgrammaticContent(TextProgrammaticContent, BinaryProgrammaticContent);
#else
[System.Runtime.CompilerServices.Union]
public readonly struct ProgrammaticContent : System.Runtime.CompilerServices.IUnion
{
    public object? Value { get; }
    public ProgrammaticContent(TextProgrammaticContent value) { Value = value; }
    public ProgrammaticContent(BinaryProgrammaticContent value) { Value = value; }
    public static implicit operator ProgrammaticContent(TextProgrammaticContent value) => new(value);
    public static implicit operator ProgrammaticContent(BinaryProgrammaticContent value) => new(value);
}
#endif

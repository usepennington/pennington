namespace Pennington.Content;

using Routing;

/// <summary>
/// A dynamically generated file (e.g., search index, RSS feed).
/// </summary>
/// <param name="OutputPath">Destination path in the generated site.</param>
/// <param name="ContentGenerator">Produces the file bytes when invoked.</param>
/// <param name="ContentType">MIME type served for the generated file.</param>
public record ContentToCreate(FilePath OutputPath, Func<Task<byte[]>> ContentGenerator, string ContentType);
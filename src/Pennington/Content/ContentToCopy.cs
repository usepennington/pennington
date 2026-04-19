namespace Pennington.Content;

using Routing;

/// <summary>
/// A static file to copy to the output directory.
/// </summary>
/// <param name="SourcePath">Source file to copy.</param>
/// <param name="OutputPath">Destination path in the generated site.</param>
public record ContentToCopy(FilePath SourcePath, FilePath OutputPath);
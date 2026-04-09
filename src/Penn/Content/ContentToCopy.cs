namespace Pennington.Content;

using Pennington.Routing;

/// <summary>
/// A static file to copy to the output directory.
/// </summary>
public record ContentToCopy(FilePath SourcePath, FilePath OutputPath);

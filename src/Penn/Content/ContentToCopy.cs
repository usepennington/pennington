namespace Penn.Content;

using Penn.Routing;

/// <summary>
/// A static file to copy to the output directory.
/// </summary>
public record ContentToCopy(FilePath SourcePath, FilePath OutputPath);

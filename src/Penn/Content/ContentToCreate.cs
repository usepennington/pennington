namespace Penn.Content;

using Penn.Routing;

/// <summary>
/// A dynamically generated file (e.g., search index, RSS feed).
/// </summary>
public record ContentToCreate(FilePath OutputPath, Func<Task<byte[]>> ContentGenerator, string ContentType);

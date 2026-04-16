namespace Pennington.Pipeline;

/// <summary>Describes a failure encountered while processing a content item.</summary>
/// <param name="Message">Human-readable error message.</param>
/// <param name="Exception">Underlying exception, if any.</param>
public record ContentError(string Message, Exception? Exception = null);

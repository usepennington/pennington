namespace Pennington.Pipeline;

public record ContentError(string Message, Exception? Exception = null);
namespace Pennington.Pipeline;

using Routing;

/// <summary>A resolvable cross-reference target identified by a unique id.</summary>
/// <param name="Uid">Unique identifier used in <c>xref:</c> links.</param>
/// <param name="Title">Display title for the target.</param>
/// <param name="Route">Route the reference resolves to.</param>
public record CrossReference(string Uid, string Title, ContentRoute Route);

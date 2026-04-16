namespace Pennington.Pipeline;

/// <summary>A single heading entry in a page outline.</summary>
/// <param name="Id">Anchor id for the heading.</param>
/// <param name="Text">Heading text.</param>
/// <param name="Level">Heading level (1–6).</param>
public record OutlineEntry(string Id, string Text, int Level);

namespace Pennington.Navigation;

using Routing;

/// <summary>One segment in a page's breadcrumb trail.</summary>
/// <param name="Title">Display title for the segment.</param>
/// <param name="Route">Route the segment links to, or null for a non-linked label.</param>
public record BreadcrumbItem(string Title, ContentRoute? Route);
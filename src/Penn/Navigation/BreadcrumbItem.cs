namespace Pennington.Navigation;

using Pennington.Routing;

public record BreadcrumbItem(string Title, ContentRoute? Route);

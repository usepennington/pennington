namespace Penn.Navigation;

using Penn.Routing;

public record BreadcrumbItem(string Title, ContentRoute? Route);

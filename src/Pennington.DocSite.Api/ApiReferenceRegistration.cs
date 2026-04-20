namespace Pennington.DocSite.Api;

/// <summary>One named API-reference tree: a provider key and the public URL prefix under which its pages are served.</summary>
/// <param name="Name">Registration name. Matches the <c>name</c> argument passed to the metadata backend's <c>AddApiMetadataFrom*</c> call.</param>
/// <param name="RoutePrefix">Normalized URL prefix with leading and trailing slashes, e.g. <c>"/api/"</c> or <c>"/reference/api/spectre/"</c>.</param>
/// <param name="TocTitle">Sidebar title for the index page's TOC entry, or <see langword="null"/> to suppress the entry.</param>
/// <param name="TocSectionLabel">Section label the TOC entry groups under, or <see langword="null"/> to keep it unsectioned.</param>
public sealed record ApiReferenceRegistration(string Name, string RoutePrefix, string? TocTitle, string? TocSectionLabel);

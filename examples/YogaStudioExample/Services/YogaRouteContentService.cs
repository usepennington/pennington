namespace YogaStudioExample.Services;

using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Emits routes that are not discovered by <c>RazorPageContentService</c>:
/// <list type="bullet">
///   <item>Parameterized instructor and class detail pages (<c>/instructors/{slug}</c>,
///     <c>/schedule/{id}</c>), skipped by the Razor scanner because their templates
///     contain a route parameter.</item>
///   <item>Locale-prefixed copies of every Razor-hosted page for non-default locales
///     (<c>/gen-z/</c>, <c>/gen-z/schedule/</c>, etc.). The Razor scanner only emits
///     default-locale variants; without these explicit entries the static crawler
///     never fetches the localized URLs and the LanguageSwitcher's cross-locale
///     links dangle.</item>
/// </list>
/// All routes round-trip through Blazor's existing <c>@page</c> handlers — the
/// locale middleware strips the <c>/gen-z/</c> prefix so the same component
/// renders for both locales.
/// </summary>
public sealed class YogaRouteContentService : IContentService
{
    private readonly InstructorService _instructors;
    private readonly ScheduleService _schedule;

    public YogaRouteContentService(InstructorService instructors, ScheduleService schedule)
    {
        _instructors = instructors;
        _schedule = schedule;
    }

    public string DefaultSection => "";
    public int SearchPriority => 5;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var route in EnumerateRoutes())
        {
            yield return new DiscoveredItem(route, new RazorPageSource(nameof(YogaRouteContentService)));
        }
        await Task.CompletedTask;
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    private IEnumerable<ContentRoute> EnumerateRoutes()
    {
        // Default-locale parameterized details (not discovered by the Razor scanner).
        foreach (var instructor in _instructors.GetAll())
            yield return ContentRouteFactory.FromRazorPage($"/instructors/{instructor.Slug}");

        foreach (var entry in _schedule.GetAllClasses())
            yield return ContentRouteFactory.FromRazorPage($"/schedule/{entry.Id}");

        // Locale-prefixed variants for every non-default locale. Gen-z is the
        // only one today, but this loop keeps adding new locales cheap.
        foreach (var locale in NonDefaultLocales)
        {
            foreach (var template in LocalizedRazorPageTemplates)
                yield return ContentRouteFactory.FromRazorPage(template, locale);

            foreach (var instructor in _instructors.GetAll())
                yield return ContentRouteFactory.FromRazorPage($"/instructors/{instructor.Slug}", locale);

            foreach (var entry in _schedule.GetAllClasses())
                yield return ContentRouteFactory.FromRazorPage($"/schedule/{entry.Id}", locale);
        }
    }

    // Hard-coded non-default locales. Program.cs configures "en" (default) and "gen-z".
    private static readonly string[] NonDefaultLocales = ["gen-z"];

    // Every top-level Razor @page route that has a locale-prefixed counterpart.
    // Must stay in sync with Components/Pages/*.razor.
    private static readonly string[] LocalizedRazorPageTemplates =
    [
        "/",
        "/schedule",
        "/instructors",
        "/blog",
        "/about",
        "/contact",
        "/faq",
        "/pricing",
    ];
}
namespace ForgePortalExample;

using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;

public class ReleaseNotesContentService : IContentService
{
    private readonly string _releasesPath;

    public ReleaseNotesContentService(IWebHostEnvironment env)
    {
        _releasesPath = Path.Combine(env.ContentRootPath, "releases");
    }

    public string DefaultSection => "Releases";
    public int SearchPriority => 50;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        if (!Directory.Exists(_releasesPath)) yield break;

        foreach (var file in Directory.GetFiles(_releasesPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var release = JsonSerializer.Deserialize<ReleaseNote>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (release is null) continue;

            var slug = $"releases/{Path.GetFileNameWithoutExtension(file).Replace(".", "-")}";
            var route = new ContentRoute
            {
                CanonicalPath = new UrlPath($"/{slug}/"),
                OutputFile = new FilePath($"{slug}/index.html"),
            };

            var generator = new ReleaseNoteGenerator(release);
            yield return new DiscoveredItem(route, new ContentSource(new ProgrammaticSource(generator)));
        }
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var items = new List<ContentTocItem>();
        if (!Directory.Exists(_releasesPath)) return items.ToImmutableList();

        foreach (var file in Directory.GetFiles(_releasesPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var release = JsonSerializer.Deserialize<ReleaseNote>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (release is null) continue;

            var slug = $"releases/{Path.GetFileNameWithoutExtension(file).Replace(".", "-")}";
            var route = new ContentRoute
            {
                CanonicalPath = new UrlPath($"/{slug}/"),
                OutputFile = new FilePath($"{slug}/index.html"),
            };

            items.Add(new ContentTocItem(
                $"v{release.Version}",
                route,
                Order: 0,
                HierarchyParts: ["Releases", $"v{release.Version}"],
                Section: "Releases",
                Locale: null));
        }

        return items.ToImmutableList();
    }

    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var refs = new List<CrossReference>();
        if (!Directory.Exists(_releasesPath)) return refs.ToImmutableList();

        foreach (var file in Directory.GetFiles(_releasesPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var release = JsonSerializer.Deserialize<ReleaseNote>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (release is null) continue;

            var slug = $"releases/{Path.GetFileNameWithoutExtension(file).Replace(".", "-")}";
            var route = new ContentRoute
            {
                CanonicalPath = new UrlPath($"/{slug}/"),
                OutputFile = new FilePath($"{slug}/index.html"),
            };

            refs.Add(new CrossReference($"forge.release.v{release.Version}", $"v{release.Version}", route));
        }

        return refs.ToImmutableList();
    }
}

file class ReleaseNoteGenerator : IProgrammaticContentGenerator
{
    private readonly ReleaseNote _release;

    public ReleaseNoteGenerator(ReleaseNote release) => _release = release;

    public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<h1>Release v{_release.Version}</h1>");
        sb.AppendLine($"<p class=\"text-base-500\">{_release.Date:MMMM d, yyyy}</p>");
        sb.AppendLine($"<p>{_release.Summary}</p>");
        sb.AppendLine("<h2>Changes</h2>");
        sb.AppendLine("<ul>");
        foreach (var change in _release.Changes)
        {
            sb.AppendLine($"<li>{change}</li>");
        }
        sb.AppendLine("</ul>");

        var fm = new PageFrontMatter { Title = $"Release v{_release.Version}" };
        var textContent = new TextProgrammaticContent(
            fm,
            sb.ToString(),
            "text/html");

        return Task.FromResult(new ProgrammaticContent(textContent));
    }
}

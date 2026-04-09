using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Bogus;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;

namespace SearchExample.Services;

public class RandomContentService : IContentService
{
    private readonly MarkdownPipeline _pipeline;
    private readonly List<(ContentRoute Route, string Title)> _items;
    private readonly Dictionary<string, string> _content = new();

    public RandomContentService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .Build();

        var faker = new Faker { Random = new Randomizer(1229) };

        const int count = 1000;
        _items = [];
        var textInfo = new CultureInfo("en-US", false).TextInfo;

        for (var i = 0; i < count; i++)
        {
            var parts = new[] { faker.Hacker.IngVerb(), faker.Hacker.Adjective() + " " + faker.Hacker.Noun(), faker.Hacker.Noun(), faker.Random.AlphaNumeric(10) };
            var title = textInfo.ToTitleCase(string.Join(' ', parts.Take(parts.Length - 1)));
            var urlRootedPath = "/" + string.Join('/', parts);
            var url = "/random" + urlRootedPath;

            _content.Add(urlRootedPath.Trim('/'), GetContentForUrl(urlRootedPath, title));

            var route = ContentRouteFactory.FromCustom(new UrlPath(url));
            _items.Add((route, title));
        }
    }

    public string DefaultSection => "";
    public int SearchPriority => 1;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var (route, title) in _items)
        {
            var generator = new RandomContentGenerator(this, title);
            ContentSource source = new ProgrammaticSource(generator);
            yield return new DiscoveredItem(route, source);
        }

        await Task.CompletedTask;
    }

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    public string GetContent(string url)
    {
        return _content.GetValueOrDefault(url.Trim('/'), "not found");
    }

    private string GetContentForUrl(string url, string title)
    {
        var faker = new Faker { Random = new Randomizer(url.GetHashCode()) };

        var sb = new StringBuilder($"# {title}");
        sb.AppendLine();
        sb.AppendLine();

        sb.AppendLine($"{GetParagraph(faker)}");
        sb.AppendLine();
        sb.AppendLine();
        var sectionCount = faker.Random.Int(2, 5);
        for (var i = 0; i < sectionCount; i++)
        {
            var sectionHeader = GetHeader(faker);
            sb.AppendLine($"## {sectionHeader}");
            sb.AppendLine();
            sb.AppendLine();

            var paragraphCount = faker.Random.Int(2, 4);
            for (var j = 0; j < paragraphCount; j++)
            {
                sb.AppendLine($"{GetParagraph(faker)}");
                sb.AppendLine();
                sb.AppendLine();
            }

            var subHeadingCount = faker.Random.Int(0, 3);
            for (var k = 0; k < subHeadingCount; k++)
            {
                var subHeader = GetHeader(faker);
                sb.AppendLine($"### {subHeader}");
                sb.AppendLine();
                sb.AppendLine();

                var subParagraphCount = faker.Random.Int(2, 4);
                for (var l = 0; l < subParagraphCount; l++)
                {
                    sb.AppendLine($"{GetParagraph(faker)}");
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }
        }

        return Markdown.ToHtml(sb.ToString(), _pipeline);
    }

    private static string GetHeader(Faker faker) => faker.Lorem.Sentence(2, 3);
    private static string GetParagraph(Faker faker) => faker.Lorem.Paragraph();

    private sealed class RandomContentGenerator(
        RandomContentService service,
        string title) : IProgrammaticContentGenerator
    {
        public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
        {
            // Extract the relative path from the route (remove /random/ prefix)
            var urlPath = route.CanonicalPath.Value.TrimStart('/');
            var relativePath = urlPath.StartsWith("random/", StringComparison.OrdinalIgnoreCase)
                ? urlPath["random/".Length..]
                : urlPath;

            var html = service.GetContent(relativePath);
            var metadata = new RandomFrontMatter { Title = title };
            ProgrammaticContent content = new TextProgrammaticContent(metadata, html, "text/html");
            return Task.FromResult(content);
        }
    }
}

public record RandomFrontMatter : IFrontMatter
{
    public required string Title { get; init; }
}

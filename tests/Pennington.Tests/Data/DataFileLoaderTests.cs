using Pennington.Data;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Data;

public class DataFileLoaderTests
{
    public record Sponsor
    {
        public string Name { get; init; } = "";
        public string Tier { get; init; } = "";
        public string Url { get; init; } = "";
    }

    public record Schedule
    {
        public string Title { get; init; } = "";
        public List<Talk> Talks { get; init; } = [];
    }

    public record Talk
    {
        public string Title { get; init; } = "";
        public string Speaker { get; init; } = "";
        public string Time { get; init; } = "";
    }

    [Fact]
    public void Load_DeserializesYaml_WithCamelCaseKeys()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/sponsors.yml",
            """
            - name: Acme Corp
              tier: gold
              url: https://acme.example
            - name: Globex
              tier: silver
              url: https://globex.example
            """);

        var sponsors = DataFileLoader.Load<List<Sponsor>>("/data/sponsors.yml", fs);

        sponsors.Count.ShouldBe(2);
        sponsors[0].Name.ShouldBe("Acme Corp");
        sponsors[0].Tier.ShouldBe("gold");
        sponsors[1].Url.ShouldBe("https://globex.example");
    }

    [Fact]
    public void Load_DeserializesJson_WithCamelCaseKeys()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/schedule.json",
            """
            {
              "title": "Devcon 2026",
              "talks": [
                { "title": "Keynote", "speaker": "Ada", "time": "09:00" },
                { "title": "Workshop", "speaker": "Lin", "time": "11:00" }
              ]
            }
            """);

        var schedule = DataFileLoader.Load<Schedule>("/data/schedule.json", fs);

        schedule.Title.ShouldBe("Devcon 2026");
        schedule.Talks.Count.ShouldBe(2);
        schedule.Talks[0].Speaker.ShouldBe("Ada");
    }

    [Fact]
    public void Load_AcceptsYamlExtension()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/nav.yaml", "title: Home\n");

        var nav = DataFileLoader.Load<Dictionary<string, string>>("/data/nav.yaml", fs);

        nav["title"].ShouldBe("Home");
    }

    [Fact]
    public void Load_ThrowsFileNotFound_WhenMissing()
    {
        var fs = new MockFileSystem();

        Should.Throw<FileNotFoundException>(() =>
            DataFileLoader.Load<List<Sponsor>>("/data/missing.yml", fs));
    }

    [Fact]
    public void Load_ThrowsNotSupported_ForUnknownExtension()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/sponsors.toml", "name = 'Acme'");

        Should.Throw<NotSupportedException>(() =>
            DataFileLoader.Load<Sponsor>("/data/sponsors.toml", fs));
    }

    [Fact]
    public void Load_WrapsDeserializationFailure_AsInvalidData()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/broken.json", "{ this is not json");

        var ex = Should.Throw<InvalidDataException>(() =>
            DataFileLoader.Load<Schedule>("/data/broken.json", fs));
        ex.Message.ShouldContain("/data/broken.json");
    }
}

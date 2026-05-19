using Pennington.Data;
using Pennington.Infrastructure;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Data;

public class DataFileEntryTests
{
    public record Sponsor
    {
        public string Name { get; init; } = "";
        public string Tier { get; init; } = "";
    }

    private static MockFileSystem WithSponsors(string content)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/sponsors.yml", content);
        return fs;
    }

    [Fact]
    public void GetValue_LoadsLazily()
    {
        var fs = WithSponsors("- name: Acme\n  tier: gold\n");

        var entry = new DataFileEntry<List<Sponsor>>("sponsors", "/data/sponsors.yml", fs);

        entry.Name.ShouldBe("sponsors");
        entry.ValueType.ShouldBe(typeof(List<Sponsor>));

        var sponsors = (List<Sponsor>)entry.GetValue();
        sponsors.Single().Name.ShouldBe("Acme");
    }

    [Fact]
    public void GetValue_CachesAcrossCalls()
    {
        var fs = WithSponsors("- name: Acme\n  tier: gold\n");

        var entry = new DataFileEntry<List<Sponsor>>("sponsors", "/data/sponsors.yml", fs);

        ReferenceEquals(entry.GetValue(), entry.GetValue()).ShouldBeTrue();
    }

    [Fact]
    public void OnFileChanged_ReloadsValue()
    {
        var fs = WithSponsors("- name: Acme\n  tier: gold\n");
        var entry = new DataFileEntry<List<Sponsor>>("sponsors", "/data/sponsors.yml", fs);

        ((List<Sponsor>)entry.GetValue()).Single().Name.ShouldBe("Acme");

        fs.File.WriteAllText("/data/sponsors.yml", "- name: Globex\n  tier: silver\n");
        var response = entry.OnFileChanged(
            new FileChangeNotification(fs.Path.GetFullPath("/data/sponsors.yml"), WatcherChangeTypes.Changed));

        response.ShouldBe(FileWatchResponse.Refreshed);
        ((List<Sponsor>)entry.GetValue()).Single().Name.ShouldBe("Globex");
    }

    [Fact]
    public void OnFileChanged_IgnoresADifferentFile()
    {
        var fs = WithSponsors("- name: Acme\n  tier: gold\n");
        var entry = new DataFileEntry<List<Sponsor>>("sponsors", "/data/sponsors.yml", fs);

        var response = entry.OnFileChanged(
            new FileChangeNotification(fs.Path.GetFullPath("/data/nav.yml"), WatcherChangeTypes.Changed));

        response.ShouldBe(FileWatchResponse.Ignore);
    }

    [Fact]
    public void WatchScopes_CoversTheDataFile()
    {
        var fs = WithSponsors("- name: Acme\n  tier: gold\n");
        var entry = new DataFileEntry<List<Sponsor>>("sponsors", "/data/sponsors.yml", fs);

        var scope = entry.WatchScopes.ShouldHaveSingleItem();
        scope.Pattern.ShouldBe("sponsors.yml");
    }
}

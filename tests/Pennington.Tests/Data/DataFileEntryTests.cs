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

    [Fact]
    public void GetValue_LoadsLazily()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/sponsors.yml", "- name: Acme\n  tier: gold\n");

        var entry = new DataFileEntry<List<Sponsor>>("sponsors", "/data/sponsors.yml", fs, new FileWatcher(fs));

        entry.Name.ShouldBe("sponsors");
        entry.ValueType.ShouldBe(typeof(List<Sponsor>));

        var sponsors = (List<Sponsor>)entry.GetValue();
        sponsors.Single().Name.ShouldBe("Acme");
    }

    [Fact]
    public void GetValue_CachesAcrossCalls()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/sponsors.yml", "- name: Acme\n  tier: gold\n");

        var entry = new DataFileEntry<List<Sponsor>>("sponsors", "/data/sponsors.yml", fs, new FileWatcher(fs));

        var first = entry.GetValue();
        var second = entry.GetValue();

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public async Task GetValue_ReloadsAfterFileChange()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/sponsors.yml", "- name: Acme\n  tier: gold\n");

        var watcher = new FileWatcher(fs);
        var entry = new DataFileEntry<List<Sponsor>>("sponsors", "/data/sponsors.yml", fs, watcher);

        var initial = (List<Sponsor>)entry.GetValue();
        initial.Single().Name.ShouldBe("Acme");

        // Rewrite the file and wait briefly for the watcher to invalidate.
        fs.File.WriteAllText("/data/sponsors.yml", "- name: Globex\n  tier: silver\n");

        var deadline = DateTime.UtcNow.AddSeconds(2);
        List<Sponsor> reloaded;
        do
        {
            await Task.Delay(50, TestContext.Current.CancellationToken);
            reloaded = (List<Sponsor>)entry.GetValue();
        } while (reloaded.Single().Name == "Acme" && DateTime.UtcNow < deadline);

        reloaded.Single().Name.ShouldBe("Globex");
        reloaded.Single().Tier.ShouldBe("silver");
    }
}

using Pennington.Data;
using Pennington.Infrastructure;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Data;

public class DataDirectoryEntryTests
{
    public record Maintainer
    {
        public string Name { get; init; } = "";
        public string GitHubUserName { get; init; } = "";
    }

    [Fact]
    public void GetValue_AggregatesFiles_OrderedByName()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/devlead.yml", "name: Mattias\ngitHubUserName: devlead\n");
        fs.File.WriteAllText("/data/maintainers/agc93.yml", "name: Alistair\ngitHubUserName: agc93\n");

        var entry = new DataDirectoryEntry<Maintainer>("maintainers", "/data/maintainers", fs, new FileWatcher(fs));

        entry.Name.ShouldBe("maintainers");
        entry.ValueType.ShouldBe(typeof(IReadOnlyList<Maintainer>));

        var maintainers = (IReadOnlyList<Maintainer>)entry.GetValue();
        maintainers.Select(m => m.GitHubUserName).ShouldBe(["agc93", "devlead"]);
        maintainers[0].Name.ShouldBe("Alistair");
    }

    [Fact]
    public void GetValue_FlattensFilesContainingArrays()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/single.yml", "name: Solo\ngitHubUserName: solo\n");
        fs.File.WriteAllText("/data/maintainers/team.yml",
            "- name: One\n  gitHubUserName: one\n- name: Two\n  gitHubUserName: two\n");

        var entry = new DataDirectoryEntry<Maintainer>("maintainers", "/data/maintainers", fs, new FileWatcher(fs));

        var maintainers = (IReadOnlyList<Maintainer>)entry.GetValue();
        maintainers.Select(m => m.GitHubUserName).ShouldBe(["solo", "one", "two"]);
    }

    [Fact]
    public void GetValue_MixesYamlAndJson()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/a.yml", "name: FromYaml\ngitHubUserName: a\n");
        fs.File.WriteAllText("/data/maintainers/b.json", """{ "name": "FromJson", "gitHubUserName": "b" }""");

        var entry = new DataDirectoryEntry<Maintainer>("maintainers", "/data/maintainers", fs, new FileWatcher(fs));

        var maintainers = (IReadOnlyList<Maintainer>)entry.GetValue();
        maintainers.Count.ShouldBe(2);
        maintainers[0].Name.ShouldBe("FromYaml");
        maintainers[1].Name.ShouldBe("FromJson");
    }

    [Fact]
    public void GetValue_IgnoresNonDataFiles()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/agc93.yml", "name: Alistair\ngitHubUserName: agc93\n");
        fs.File.WriteAllText("/data/maintainers/README.md", "# Maintainers");

        var entry = new DataDirectoryEntry<Maintainer>("maintainers", "/data/maintainers", fs, new FileWatcher(fs));

        var maintainers = (IReadOnlyList<Maintainer>)entry.GetValue();
        maintainers.Single().GitHubUserName.ShouldBe("agc93");
    }

    [Fact]
    public void GetValue_LoadsLazily()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");

        // Directory does not exist yet at construction; constructing must not throw.
        var entry = new DataDirectoryEntry<Maintainer>("maintainers", "/data/maintainers", fs, new FileWatcher(fs));

        fs.File.WriteAllText("/data/maintainers/agc93.yml", "name: Alistair\ngitHubUserName: agc93\n");

        var maintainers = (IReadOnlyList<Maintainer>)entry.GetValue();
        maintainers.Single().GitHubUserName.ShouldBe("agc93");
    }

    [Fact]
    public void GetValue_ThrowsDirectoryNotFound_WhenMissing()
    {
        var fs = new MockFileSystem();

        var entry = new DataDirectoryEntry<Maintainer>("maintainers", "/data/maintainers", fs, new FileWatcher(fs));

        Should.Throw<DirectoryNotFoundException>(() => entry.GetValue());
    }

    [Fact]
    public async Task GetValue_ReloadsAfterFileAdded()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/agc93.yml", "name: Alistair\ngitHubUserName: agc93\n");

        var watcher = new FileWatcher(fs);
        var entry = new DataDirectoryEntry<Maintainer>("maintainers", "/data/maintainers", fs, watcher);

        ((IReadOnlyList<Maintainer>)entry.GetValue()).Count.ShouldBe(1);

        fs.File.WriteAllText("/data/maintainers/devlead.yml", "name: Mattias\ngitHubUserName: devlead\n");

        var deadline = DateTime.UtcNow.AddSeconds(2);
        IReadOnlyList<Maintainer> reloaded;
        do
        {
            await Task.Delay(50, TestContext.Current.CancellationToken);
            reloaded = (IReadOnlyList<Maintainer>)entry.GetValue();
        } while (reloaded.Count == 1 && DateTime.UtcNow < deadline);

        reloaded.Select(m => m.GitHubUserName).ShouldBe(["agc93", "devlead"]);
    }
}

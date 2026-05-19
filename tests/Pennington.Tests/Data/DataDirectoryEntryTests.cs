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

    private static DataDirectoryEntry<Maintainer> Entry(MockFileSystem fs) =>
        new("maintainers", "/data/maintainers", fs);

    [Fact]
    public void GetValue_AggregatesFiles_OrderedByName()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/devlead.yml", "name: Mattias\ngitHubUserName: devlead\n");
        fs.File.WriteAllText("/data/maintainers/agc93.yml", "name: Alistair\ngitHubUserName: agc93\n");

        var entry = Entry(fs);

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

        var maintainers = (IReadOnlyList<Maintainer>)Entry(fs).GetValue();
        maintainers.Select(m => m.GitHubUserName).ShouldBe(["solo", "one", "two"]);
    }

    [Fact]
    public void GetValue_MixesYamlAndJson()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/a.yml", "name: FromYaml\ngitHubUserName: a\n");
        fs.File.WriteAllText("/data/maintainers/b.json", """{ "name": "FromJson", "gitHubUserName": "b" }""");

        var maintainers = (IReadOnlyList<Maintainer>)Entry(fs).GetValue();
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

        var maintainers = (IReadOnlyList<Maintainer>)Entry(fs).GetValue();
        maintainers.Single().GitHubUserName.ShouldBe("agc93");
    }

    [Fact]
    public void GetValue_LoadsLazily()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");

        // Constructed before any file exists; construction must not read the directory.
        var entry = Entry(fs);
        fs.File.WriteAllText("/data/maintainers/agc93.yml", "name: Alistair\ngitHubUserName: agc93\n");

        var maintainers = (IReadOnlyList<Maintainer>)entry.GetValue();
        maintainers.Single().GitHubUserName.ShouldBe("agc93");
    }

    [Fact]
    public void GetValue_ThrowsDirectoryNotFound_WhenMissing()
    {
        var fs = new MockFileSystem();

        Should.Throw<DirectoryNotFoundException>(() => Entry(fs).GetValue());
    }

    [Fact]
    public void OnFileChanged_ReloadsAfterFileAdded()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/agc93.yml", "name: Alistair\ngitHubUserName: agc93\n");

        var entry = Entry(fs);
        ((IReadOnlyList<Maintainer>)entry.GetValue()).Count.ShouldBe(1);

        fs.File.WriteAllText("/data/maintainers/devlead.yml", "name: Mattias\ngitHubUserName: devlead\n");
        var response = entry.OnFileChanged(new FileChangeNotification(
            fs.Path.GetFullPath("/data/maintainers/devlead.yml"), WatcherChangeTypes.Created));

        response.ShouldBe(FileWatchResponse.Refreshed);
        ((IReadOnlyList<Maintainer>)entry.GetValue())
            .Select(m => m.GitHubUserName).ShouldBe(["agc93", "devlead"]);
    }

    [Fact]
    public void OnFileChanged_IgnoresChangesOutsideTheDirectory()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/maintainers");
        fs.File.WriteAllText("/data/maintainers/agc93.yml", "name: Alistair\ngitHubUserName: agc93\n");

        var response = Entry(fs).OnFileChanged(new FileChangeNotification(
            fs.Path.GetFullPath("/data/other/x.yml"), WatcherChangeTypes.Changed));

        response.ShouldBe(FileWatchResponse.Ignore);
    }
}

using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pennington.Data;
using Pennington.Infrastructure;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Data;

public class DataFileServiceExtensionsTests
{
    public record Sponsor
    {
        public string Name { get; init; } = "";
        public string Tier { get; init; } = "";
    }

    public record NavLink
    {
        public string Label { get; init; } = "";
        public string Href { get; init; } = "";
    }

    private static ServiceProvider BuildProvider(MockFileSystem fs, Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IFileSystem>(fs);
        services.AddSingleton<IFileWatcher>(sp => new FileWatcher(fs));
        register(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddDataFile_ResolvesIDataFiles_ByName()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/sponsors.yml", "- name: Acme\n  tier: gold\n");

        using var provider = BuildProvider(fs, services =>
            services.AddDataFile<List<Sponsor>>("sponsors", "/data/sponsors.yml"));

        var data = provider.GetRequiredService<IDataFiles>();
        data.Get<List<Sponsor>>("sponsors").Single().Name.ShouldBe("Acme");
    }

    [Fact]
    public void AddDataFile_RegistersMultipleFiles()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/sponsors.yml", "- name: Acme\n  tier: gold\n");
        fs.File.WriteAllText("/data/nav.json",
            """[{"label":"Home","href":"/"},{"label":"Docs","href":"/docs"}]""");

        using var provider = BuildProvider(fs, services =>
        {
            services.AddDataFile<List<Sponsor>>("sponsors", "/data/sponsors.yml");
            services.AddDataFile<List<NavLink>>("nav", "/data/nav.json");
        });

        var data = provider.GetRequiredService<IDataFiles>();
        data.Names.ShouldBe(["sponsors", "nav"], ignoreOrder: true);
        data.Get<List<NavLink>>("nav").Count.ShouldBe(2);
    }

    [Fact]
    public void AddDataFile_OnlyRegistersAggregatorOnce()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.File.WriteAllText("/data/a.yml", "- a\n");
        fs.File.WriteAllText("/data/b.yml", "- b\n");

        using var provider = BuildProvider(fs, services =>
        {
            services.AddDataFile<List<string>>("a", "/data/a.yml");
            services.AddDataFile<List<string>>("b", "/data/b.yml");
        });

        var first = provider.GetRequiredService<IDataFiles>();
        var second = provider.GetRequiredService<IDataFiles>();

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void AddDataDirectory_ResolvesIDataFiles_AsReadOnlyList()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data/sponsors");
        fs.File.WriteAllText("/data/sponsors/acme.yml", "name: Acme\ntier: gold\n");
        fs.File.WriteAllText("/data/sponsors/globex.yml", "name: Globex\ntier: silver\n");

        using var provider = BuildProvider(fs, services =>
            services.AddDataDirectory<Sponsor>("sponsors", "/data/sponsors"));

        var data = provider.GetRequiredService<IDataFiles>();
        data.Get<IReadOnlyList<Sponsor>>("sponsors").Select(s => s.Name)
            .ShouldBe(["Acme", "Globex"]);
    }

    [Fact]
    public void AddDataDirectory_CoexistsWithAddDataFile()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");
        fs.Directory.CreateDirectory("/data/sponsors");
        fs.File.WriteAllText("/data/nav.json",
            """[{"label":"Home","href":"/"}]""");
        fs.File.WriteAllText("/data/sponsors/acme.yml", "name: Acme\ntier: gold\n");

        using var provider = BuildProvider(fs, services =>
        {
            services.AddDataFile<List<NavLink>>("nav", "/data/nav.json");
            services.AddDataDirectory<Sponsor>("sponsors", "/data/sponsors");
        });

        var data = provider.GetRequiredService<IDataFiles>();
        data.Names.ShouldBe(["nav", "sponsors"], ignoreOrder: true);
        data.Get<List<NavLink>>("nav").Single().Label.ShouldBe("Home");
        data.Get<IReadOnlyList<Sponsor>>("sponsors").Single().Name.ShouldBe("Acme");
    }
}

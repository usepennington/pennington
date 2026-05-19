using Pennington.Data;
using Pennington.Infrastructure;

namespace Pennington.Tests.Data;

public class DataFilesTests
{
    private sealed class FakeDataFile(string name, Type type, object value) : IDataFile
    {
        public string Name { get; } = name;
        public Type ValueType { get; } = type;
        public object GetValue() => value;
        public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Ignore;
    }

    [Fact]
    public void Get_ReturnsTypedValue_ByName()
    {
        var sponsors = new List<string> { "Acme", "Globex" };
        var nav = new Dictionary<string, string> { ["home"] = "/" };
        var registry = new DataFiles([
            new FakeDataFile("sponsors", typeof(List<string>), sponsors),
            new FakeDataFile("nav", typeof(Dictionary<string, string>), nav),
        ]);

        registry.Get<List<string>>("sponsors").ShouldBe(sponsors);
        registry.Get<Dictionary<string, string>>("nav")["home"].ShouldBe("/");
    }

    [Fact]
    public void Get_IsCaseInsensitive()
    {
        var registry = new DataFiles([
            new FakeDataFile("Sponsors", typeof(List<string>), new List<string> { "Acme" }),
        ]);

        registry.Get<List<string>>("sponsors").Single().ShouldBe("Acme");
        registry.Get<List<string>>("SPONSORS").Single().ShouldBe("Acme");
    }

    [Fact]
    public void Get_ThrowsKeyNotFound_WhenNameMissing()
    {
        var registry = new DataFiles([
            new FakeDataFile("sponsors", typeof(List<string>), new List<string>()),
        ]);

        var ex = Should.Throw<KeyNotFoundException>(() => registry.Get<List<string>>("speakers"));
        ex.Message.ShouldContain("speakers");
        ex.Message.ShouldContain("sponsors"); // lists registered names
    }

    [Fact]
    public void Get_ThrowsInvalidCast_WhenTypeMismatched()
    {
        var registry = new DataFiles([
            new FakeDataFile("sponsors", typeof(List<string>), new List<string> { "Acme" }),
        ]);

        Should.Throw<InvalidCastException>(() => registry.Get<List<int>>("sponsors"));
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenMissing()
    {
        var registry = new DataFiles([]);

        registry.TryGet<List<string>>("anything", out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenWrongType()
    {
        var registry = new DataFiles([
            new FakeDataFile("sponsors", typeof(List<string>), new List<string> { "Acme" }),
        ]);

        registry.TryGet<List<int>>("sponsors", out _).ShouldBeFalse();
    }

    [Fact]
    public void Constructor_ThrowsOnDuplicateNames()
    {
        Should.Throw<InvalidOperationException>(() => new DataFiles([
            new FakeDataFile("sponsors", typeof(List<string>), new List<string>()),
            new FakeDataFile("Sponsors", typeof(List<string>), new List<string>()),
        ]));
    }

    [Fact]
    public void Names_EnumeratesRegisteredKeys()
    {
        var registry = new DataFiles([
            new FakeDataFile("sponsors", typeof(List<string>), new List<string>()),
            new FakeDataFile("nav", typeof(Dictionary<string, string>), new Dictionary<string, string>()),
        ]);

        registry.Names.ShouldBe(["sponsors", "nav"], ignoreOrder: true);
    }
}

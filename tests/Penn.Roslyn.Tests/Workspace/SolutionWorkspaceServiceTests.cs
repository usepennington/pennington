namespace Penn.Roslyn.Tests.Workspace;

using Microsoft.Extensions.Logging.Abstractions;
using Penn.Infrastructure;
using Penn.Roslyn.Workspace;

public sealed class SolutionWorkspaceServiceTests
{
    [Fact]
    public void Implements_ISolutionWorkspaceService()
    {
        typeof(SolutionWorkspaceService)
            .GetInterfaces()
            .ShouldContain(typeof(ISolutionWorkspaceService));
    }

    [Fact]
    public void Constructor_Does_Not_Throw_With_Valid_Options()
    {
        var options = new RoslynOptions { SolutionPath = "B:\\Penn\\Penn.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        using var service = new SolutionWorkspaceService(options, watcher, logger);

        service.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_Throws_When_SolutionPath_Is_Null()
    {
        var options = new RoslynOptions { SolutionPath = null };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        Should.Throw<ArgumentException>(() =>
            new SolutionWorkspaceService(options, watcher, logger));
    }

    [Fact]
    public void InvalidateSolution_Does_Not_Throw_Before_Load()
    {
        var options = new RoslynOptions { SolutionPath = "B:\\Penn\\Penn.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        using var service = new SolutionWorkspaceService(options, watcher, logger);

        Should.NotThrow(() => service.InvalidateSolution());
    }

    [Fact]
    public void UpdateDocument_Queues_Without_Error()
    {
        var options = new RoslynOptions { SolutionPath = "B:\\Penn\\Penn.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        using var service = new SolutionWorkspaceService(options, watcher, logger);

        Should.NotThrow(() => service.UpdateDocument("B:\\Penn\\src\\Penn\\Routing\\UrlPath.cs"));
    }

    [Fact]
    public void Constructor_Registers_File_Watches()
    {
        var options = new RoslynOptions { SolutionPath = "B:\\Penn\\Penn.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        using var service = new SolutionWorkspaceService(options, watcher, logger);

        // Should register watches for *.csproj, *.sln, *.slnx, *.cs
        watcher.RegisteredPatterns.ShouldContain("*.csproj");
        watcher.RegisteredPatterns.ShouldContain("*.sln");
        watcher.RegisteredPatterns.ShouldContain("*.slnx");
        watcher.RegisteredPatterns.ShouldContain("*.cs");
    }

    [Fact]
    public void Dispose_Can_Be_Called_Multiple_Times()
    {
        var options = new RoslynOptions { SolutionPath = "B:\\Penn\\Penn.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        var service = new SolutionWorkspaceService(options, watcher, logger);

        Should.NotThrow(() =>
        {
            service.Dispose();
            service.Dispose();
        });
    }

    /// <summary>
    /// Minimal stub for IFileWatcher that records registered patterns.
    /// </summary>
    private sealed class StubFileWatcher : IFileWatcher
    {
        public List<string> RegisteredPatterns { get; } = [];

        public void AddPathWatch(string path, string filePattern, Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true)
        {
            RegisteredPatterns.Add(filePattern);
        }

        public void SubscribeToChanges(Action onUpdate) { }

        public void Dispose() { }
    }
}

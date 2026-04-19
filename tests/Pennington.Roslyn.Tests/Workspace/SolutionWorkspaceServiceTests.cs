namespace Pennington.Roslyn.Tests.Workspace;

using Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Roslyn.Symbols;
using Pennington.Roslyn.Workspace;

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
        var options = new RoslynOptions { SolutionPath = "B:\\Pennington\\Pennington.slnx" };
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
        var options = new RoslynOptions { SolutionPath = "B:\\Pennington\\Pennington.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        using var service = new SolutionWorkspaceService(options, watcher, logger);

        Should.NotThrow(() => service.InvalidateSolution());
    }

    [Fact]
    public void UpdateDocument_Queues_Without_Error()
    {
        var options = new RoslynOptions { SolutionPath = "B:\\Pennington\\Pennington.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        using var service = new SolutionWorkspaceService(options, watcher, logger);

        Should.NotThrow(() => service.UpdateDocument("B:\\Pennington\\src\\Pennington\\Routing\\UrlPath.cs"));
    }

    [Fact]
    public void Constructor_Registers_File_Watches()
    {
        var options = new RoslynOptions { SolutionPath = "B:\\Pennington\\Pennington.slnx" };
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
        var options = new RoslynOptions { SolutionPath = "B:\\Pennington\\Pennington.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;

        var service = new SolutionWorkspaceService(options, watcher, logger);

        Should.NotThrow(() =>
        {
            service.Dispose();
            service.Dispose();
        });
    }

    [Fact]
    public void InvalidateSolution_Clears_Symbol_Cache()
    {
        var options = new RoslynOptions { SolutionPath = "B:\\Pennington\\Pennington.slnx" };
        var watcher = new StubFileWatcher();
        var logger = NullLogger<SolutionWorkspaceService>.Instance;
        var symbolService = new SpySymbolExtractionService();

        using var service = new SolutionWorkspaceService(options, watcher, logger);
        service.SymbolExtractionService = symbolService;

        service.InvalidateSolution();

        symbolService.ClearCacheCallCount.ShouldBe(1);
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

        public void SubscribeToChanges(Action<FileChangeNotification> onUpdate) { }

        public void Dispose() { }
    }

    private sealed class SpySymbolExtractionService : ISymbolExtractionService
    {
        public int ClearCacheCallCount { get; private set; }

        public Task<IReadOnlyDictionary<string, Pennington.Roslyn.Symbols.SymbolInfo>> ExtractSymbolsAsync(Solution solution)
            => Task.FromResult<IReadOnlyDictionary<string, Pennington.Roslyn.Symbols.SymbolInfo>>(
                new Dictionary<string, Pennington.Roslyn.Symbols.SymbolInfo>());

        public Task<Pennington.Roslyn.Symbols.SymbolInfo?> FindSymbolAsync(string xmlDocId)
            => Task.FromResult<Pennington.Roslyn.Symbols.SymbolInfo?>(null);

        public Task<string> ExtractCodeFragmentAsync(string xmlDocId, bool bodyOnly = false, bool includeLeadingTrivia = true)
            => Task.FromResult(string.Empty);

        public Task<string> ExtractDeclarationSignatureAsync(string xmlDocId)
            => Task.FromResult(string.Empty);

        public void ClearCache() => ClearCacheCallCount++;
    }
}
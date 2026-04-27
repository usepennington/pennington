using System.Collections.Immutable;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Pennington.Content;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Generation;

public class OutputGenerationServiceTests
{
    private static OutputGenerationService CreateService(
        MockFileSystem fs,
        OutputOptions outputOptions,
        IEnumerable<IContentService>? contentServices = null)
    {
        var env = new StubWebHostEnvironment();
        var endpoints = new StubEndpointDataSource();
        var logger = NullLogger<OutputGenerationService>.Instance;

        return new OutputGenerationService(
            contentServices ?? [],
            outputOptions,
            new PenningtonOptions(),
            env,
            endpoints,
            fs,
            new StubInProcessHttpDispatcher(),
            logger);
    }

    [Fact]
    public async Task GenerateAsync_CleanOutputTrue_DeletesExistingFiles()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("output");
        fs.File.WriteAllText("output/existing.html", "<p>old</p>");

        var options = new OutputOptions
        {
            OutputDirectory = new FilePath("output"),
            CleanOutput = true,
        };

        var service = CreateService(fs, options);
        await service.GenerateAsync();

        fs.File.Exists("output/existing.html").ShouldBeFalse();
        fs.Directory.Exists("output").ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CleanOutputFalse_PreservesExistingFiles()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("output");
        fs.File.WriteAllText("output/existing.html", "<p>old</p>");

        var options = new OutputOptions
        {
            OutputDirectory = new FilePath("output"),
            CleanOutput = false,
        };

        var service = CreateService(fs, options);
        await service.GenerateAsync();

        fs.File.Exists("output/existing.html").ShouldBeTrue();
        fs.Directory.Exists("output").ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateAsync_TwoContentServices_SameOutputFile_WarnsAndDedupes()
    {
        // Regression for the parallel-write race reported in
        // postmortem-ExtensibilityLabExample.md: when two content services claim
        // the same output file, Phase 6's Parallel.ForEachAsync would fetch the
        // URL twice and both tasks would try to write the same path.
        var fs = new MockFileSystem();
        var options = new OutputOptions
        {
            OutputDirectory = new FilePath("output"),
            CleanOutput = false,
        };

        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/shared"),
            OutputFile = new FilePath("shared/index.html"),
        };

        IContentService[] services =
        [
            new StubContentService(route),
            new StubContentService(route),
        ];

        var service = CreateService(fs, options, services);
        var report = await service.GenerateAsync();

        // The duplicate must surface as a warning, not a crash.
        report.Diagnostics.Any(d =>
            d.Severity == Pennington.Diagnostics.DiagnosticSeverity.Warning &&
            d.Message.Contains("Duplicate route") &&
            d.Message.Contains("shared/index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateAsync_DuplicateWebRootEntries_CopiesEachPathOnce()
    {
        // Regression: WebRootFileProvider is a CompositeFileProvider whose children
        // (physical wwwroot + RCL manifest providers) can each expose the same
        // logical path, e.g. `_content/Pennington.UI/scripts.js`. Without dedup,
        // File.Copy ran twice on the same target and Windows returned
        // ERROR_SHARING_VIOLATION between the two calls.
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("sourceA/_content/Pennington.UI");
        fs.Directory.CreateDirectory("sourceB/_content/Pennington.UI");
        fs.File.WriteAllText("sourceA/_content/Pennington.UI/scripts.js", "A");
        fs.File.WriteAllText("sourceB/_content/Pennington.UI/scripts.js", "B");

        var providerA = new StubFileProvider("_content/Pennington.UI/scripts.js", "sourceA/_content/Pennington.UI/scripts.js");
        var providerB = new StubFileProvider("_content/Pennington.UI/scripts.js", "sourceB/_content/Pennington.UI/scripts.js");
        var composite = new Microsoft.Extensions.FileProviders.CompositeFileProvider(providerA, providerB);

        var options = new OutputOptions
        {
            OutputDirectory = new FilePath("output"),
            CleanOutput = false,
        };

        var env = new StubWebHostEnvironment { WebRootFileProvider = composite };
        var service = new OutputGenerationService(
            [],
            options,
            new PenningtonOptions(),
            env,
            new StubEndpointDataSource(),
            fs,
            new StubInProcessHttpDispatcher(),
            NullLogger<OutputGenerationService>.Instance);

        var report = await service.GenerateAsync();

        // First-wins: only providerA's copy should run. If dedup is broken, providerB
        // would overwrite and the output would be "B".
        fs.File.ReadAllText("output/_content/Pennington.UI/scripts.js").ShouldBe("A");
        report.Diagnostics.Any(d => d.Message.Contains("Failed to copy")).ShouldBeFalse();
    }

    // --- Stubs ---

    private sealed class StubContentService(ContentRoute route) : IContentService
    {
        public string DefaultSectionLabel => "Test";
        public int SearchPriority => 0;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            await Task.CompletedTask;
            yield return new DiscoveredItem(
                route,
                new ContentSource(new MarkdownFileSource(new FilePath("stub.md"))));
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
            => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
            => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
            => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
            => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    private class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "Test";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "Test";
    }

    private class StubEndpointDataSource : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints => [];

        public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
    }

    private sealed class StubInProcessHttpDispatcher : IInProcessHttpDispatcher
    {
        public HttpClient CreateClient() => new(new StubHandler())
        {
            BaseAddress = new Uri("http://stub/"),
        };

        private sealed class StubHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
                Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("<html></html>", System.Text.Encoding.UTF8, "text/html"),
                });
        }
    }

    private sealed class StubFileProvider(string relativePath, string physicalPath) : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var segments = relativePath.Split('/');
            var normalizedSubpath = subpath.Trim('/');
            var depth = string.IsNullOrEmpty(normalizedSubpath) ? 0 : normalizedSubpath.Split('/').Length;

            if (depth >= segments.Length)
                return NotFoundDirectoryContents.Singleton;

            var expectedPrefix = string.Join('/', segments.Take(depth));
            if (!string.Equals(normalizedSubpath, expectedPrefix, StringComparison.Ordinal))
                return NotFoundDirectoryContents.Singleton;

            var isLeaf = depth == segments.Length - 1;
            var entry = isLeaf
                ? (IFileInfo)new StubFileInfo(segments[depth], physicalPath, isDirectory: false)
                : new StubFileInfo(segments[depth], physicalPath: null, isDirectory: true);
            return new StubDirectoryContents(entry);
        }

        public IFileInfo GetFileInfo(string subpath) => new NotFoundFileInfo(subpath);

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
    }

    private sealed class StubDirectoryContents(IFileInfo entry) : IDirectoryContents
    {
        public bool Exists => true;
        public IEnumerator<IFileInfo> GetEnumerator() { yield return entry; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class StubFileInfo(string name, string? physicalPath, bool isDirectory) : IFileInfo
    {
        public bool Exists => true;
        public long Length => 0;
        public string? PhysicalPath => physicalPath;
        public string Name => name;
        public DateTimeOffset LastModified => DateTimeOffset.MinValue;
        public bool IsDirectory => isDirectory;
        public Stream CreateReadStream() => throw new NotSupportedException();
    }
}
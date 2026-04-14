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
        await service.GenerateAsync("http://localhost:9999");

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
        await service.GenerateAsync("http://localhost:9999");

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
        var report = await service.GenerateAsync("http://localhost:9999");

        // The duplicate must surface as a warning, not a crash.
        report.Diagnostics.Any(d =>
            d.Severity == Pennington.Diagnostics.DiagnosticSeverity.Warning &&
            d.Message.Contains("Duplicate route") &&
            d.Message.Contains("shared/index.html")).ShouldBeTrue();
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
}
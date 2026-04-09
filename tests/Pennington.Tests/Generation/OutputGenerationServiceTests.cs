using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Pennington.Content;
using Pennington.Generation;
using Pennington.Infrastructure;
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

    // --- Stubs ---

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

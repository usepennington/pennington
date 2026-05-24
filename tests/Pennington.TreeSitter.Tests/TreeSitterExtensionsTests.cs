namespace Pennington.TreeSitter.Tests;

using Microsoft.Extensions.DependencyInjection;
using Pennington.Infrastructure;
using Pennington.Markdown.Extensions;
using Pennington.TreeSitter;

public sealed class TreeSitterExtensionsTests
{
    [Fact]
    public void Registers_preprocessor_when_content_root_configured()
    {
        var services = new ServiceCollection();

        services.AddPenningtonTreeSitter(options => options.ContentRoot = ".");

        services.ShouldContain(d => d.ServiceType == typeof(ICodeBlockPreprocessor));
    }

    [Fact]
    public void Registers_file_watch_aware_when_content_root_configured()
    {
        var services = new ServiceCollection();

        services.AddPenningtonTreeSitter(options => options.ContentRoot = ".");

        services.ShouldContain(d => d.ServiceType == typeof(IFileWatchAware));
    }

    [Fact]
    public void Registers_no_preprocessor_without_content_root()
    {
        var services = new ServiceCollection();

        services.AddPenningtonTreeSitter();

        services.ShouldNotContain(d => d.ServiceType == typeof(ICodeBlockPreprocessor));
        services.ShouldNotContain(d => d.ServiceType == typeof(IFileWatchAware));
        services.ShouldContain(d => d.ServiceType == typeof(TreeSitterOptions));
    }
}

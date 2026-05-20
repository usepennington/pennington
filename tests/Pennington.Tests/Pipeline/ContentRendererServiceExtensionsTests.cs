using Microsoft.Extensions.DependencyInjection;
using Pennington.Pipeline;

namespace Pennington.Tests.Pipeline;

public class ContentRendererServiceExtensionsTests
{
    [Fact]
    public void ReplaceContentRenderer_TypeOverload_ReplacesAllPriorRegistrations()
    {
        var services = new ServiceCollection();
        services.AddTransient<IContentRenderer, OriginalRenderer>();
        services.AddTransient<IContentRenderer, AnotherRenderer>();

        services.ReplaceContentRenderer<OriginalRenderer, ReplacementRenderer>();

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetServices<IContentRenderer>().ToList();

        resolved.Count.ShouldBe(1);
        resolved[0].ShouldBeOfType<ReplacementRenderer>();
    }

    [Fact]
    public void ReplaceContentRenderer_FactoryOverload_ReplacesAllPriorRegistrations()
    {
        var services = new ServiceCollection();
        services.AddTransient<IContentRenderer, OriginalRenderer>();

        services.ReplaceContentRenderer<OriginalRenderer, ReplacementWithArgRenderer>(
            _ => new ReplacementWithArgRenderer("hello"));

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetServices<IContentRenderer>().ToList();

        resolved.Count.ShouldBe(1);
        var withArg = resolved[0].ShouldBeOfType<ReplacementWithArgRenderer>();
        withArg.Tag.ShouldBe("hello");
    }

    [Fact]
    public void ReplaceContentRenderer_NoPriorRegistration_AddsReplacement()
    {
        var services = new ServiceCollection();

        services.ReplaceContentRenderer<OriginalRenderer, ReplacementRenderer>();

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetServices<IContentRenderer>().ToList();

        resolved.Count.ShouldBe(1);
        resolved[0].ShouldBeOfType<ReplacementRenderer>();
    }

    [Fact]
    public void ReplaceContentRenderer_FactoryOverload_NullFactory_Throws()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(() =>
            services.ReplaceContentRenderer<OriginalRenderer, ReplacementRenderer>(factory: null!));
    }

    private sealed class OriginalRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item) => throw new NotSupportedException();
    }

    private sealed class AnotherRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item) => throw new NotSupportedException();
    }

    private sealed class ReplacementRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item) => throw new NotSupportedException();
    }

    private sealed class ReplacementWithArgRenderer(string tag) : IContentRenderer
    {
        public string Tag { get; } = tag;
        public Task<ContentItem> RenderAsync(ParsedItem item) => throw new NotSupportedException();
    }
}
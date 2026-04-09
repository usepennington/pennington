using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Pipeline;

public class ContentSourceTests
{
    [Fact]
    public void ExhaustivePatternMatch_AllCases()
    {
        ContentSource markdown = new MarkdownFileSource("page.md");
        ContentSource razor = new RazorPageSource("MyPage");
        ContentSource redirect = new RedirectSource("/target");
        ContentSource programmatic = new ProgrammaticSource(new StubGenerator());

        Describe(markdown).ShouldBe("Markdown: page.md");
        Describe(razor).ShouldBe("Razor: MyPage");
        Describe(redirect).ShouldBe("Redirect: /target");
        Describe(programmatic).ShouldStartWith("Programmatic:");
    }

    [Fact]
    public void UnwrapsToCorrectType_MarkdownFileSource()
    {
        ContentSource cs = new MarkdownFileSource("docs/intro.md");
        var result = cs switch
        {
            MarkdownFileSource m => m.Path.Value,
            _ => "wrong"
        };
        result.ShouldBe("docs/intro.md");
    }

    [Fact]
    public void UnwrapsToCorrectType_RazorPageSource()
    {
        ContentSource cs = new RazorPageSource("App.Pages.Home");
        var result = cs switch
        {
            RazorPageSource r => r.ComponentType,
            _ => "wrong"
        };
        result.ShouldBe("App.Pages.Home");
    }

    [Fact]
    public void UnwrapsToCorrectType_RedirectSource()
    {
        ContentSource cs = new RedirectSource("/new-path");
        var result = cs switch
        {
            RedirectSource r => r.TargetUrl.Value,
            _ => "wrong"
        };
        result.ShouldBe("/new-path");
    }

    [Fact]
    public void UnwrapsToCorrectType_ProgrammaticSource()
    {
        var gen = new StubGenerator();
        ContentSource cs = new ProgrammaticSource(gen);
        var result = cs switch
        {
            ProgrammaticSource p => p.Generator,
            _ => (IProgrammaticContentGenerator?)null
        };
        result.ShouldBe(gen);
    }

    private static string Describe(ContentSource cs) => cs switch
    {
        MarkdownFileSource m => $"Markdown: {m.Path}",
        RazorPageSource r => $"Razor: {r.ComponentType}",
        RedirectSource r => $"Redirect: {r.TargetUrl}",
        ProgrammaticSource p => $"Programmatic: {p.Generator}",
        null => throw new InvalidOperationException("Uninitialized ContentSource"),
    };

    private class StubGenerator : IProgrammaticContentGenerator
    {
        public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
            => Task.FromResult<ProgrammaticContent>(new TextProgrammaticContent(null, "test"));
    }
}

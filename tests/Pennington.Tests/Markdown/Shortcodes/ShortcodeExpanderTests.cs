namespace Pennington.Tests.Markdown.Shortcodes;

using Pennington.FrontMatter;
using Pennington.Markdown.Shortcodes;
using Pennington.Routing;

public class ShortcodeExpanderTests
{
    private static ShortcodeContext MakeContext() => new(
        new ContentRoute
        {
            CanonicalPath = new UrlPath("/test/"),
            OutputFile = new FilePath("test/index.html"),
        },
        new DocFrontMatter { Title = "Test" });

    private sealed class CapturingShortcode : IShortcode
    {
        private readonly Func<ShortcodeInvocation, string> _render;

        public CapturingShortcode(string name, Func<ShortcodeInvocation, string> render)
        {
            Name = name;
            _render = render;
        }

        public string Name { get; }

        public Task<string> ExecuteAsync(
            ShortcodeInvocation invocation,
            ShortcodeContext context,
            CancellationToken cancellationToken)
            => Task.FromResult(_render(invocation));
    }

    [Fact]
    public async Task ExpandAsync_NoShortcodeMarker_ReturnsInputUnchanged()
    {
        var expander = new ShortcodeExpander([new CapturingShortcode("Foo", _ => "x")]);

        var result = await expander.ExpandAsync(
            "Plain markdown without directives.",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldBe("Plain markdown without directives.");
    }

    [Fact]
    public async Task ExpandAsync_NoHandlersRegistered_ReturnsInputUnchanged()
    {
        var expander = new ShortcodeExpander([]);

        var result = await expander.ExpandAsync(
            "Before <?# Foo /?> After.",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldBe("Before <?# Foo /?> After.");
    }

    [Fact]
    public async Task ExpandAsync_SelfClosingShortcode_InvokesHandler()
    {
        var expander = new ShortcodeExpander([new CapturingShortcode("Greet", _ => "hello")]);

        var result = await expander.ExpandAsync(
            "Says <?# Greet /?> world.",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldBe("Says hello world.");
    }

    [Fact]
    public async Task ExpandAsync_BlockShortcode_PassesInlineContent()
    {
        var expander = new ShortcodeExpander([
            new CapturingShortcode("Wrap", inv => $"[{inv.Content}]"),
        ]);

        var result = await expander.ExpandAsync(
            "Before <?# Wrap ?>inner text<?#/ Wrap ?> after.",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldBe("Before [inner text] after.");
    }

    [Fact]
    public async Task ExpandAsync_PositionalAndNamedArgs_AreParsed()
    {
        IReadOnlyList<string>? capturedPositional = null;
        IReadOnlyDictionary<string, string>? capturedNamed = null;
        var expander = new ShortcodeExpander([
            new CapturingShortcode("Args", inv =>
            {
                capturedPositional = inv.PositionalArgs;
                capturedNamed = inv.NamedArgs;
                return "ok";
            }),
        ]);

        await expander.ExpandAsync(
            "<?# Args first second key=value other=42 /?>",
            MakeContext(),
            TestContext.Current.CancellationToken);

        capturedPositional.ShouldNotBeNull();
        capturedPositional!.ShouldBe(["first", "second"]);
        capturedNamed.ShouldNotBeNull();
        capturedNamed!["key"].ShouldBe("value");
        capturedNamed["other"].ShouldBe("42");
    }

    [Fact]
    public async Task ExpandAsync_QuotedArgWithSpaces_Preserved()
    {
        IReadOnlyDictionary<string, string>? capturedNamed = null;
        IReadOnlyList<string>? capturedPositional = null;
        var expander = new ShortcodeExpander([
            new CapturingShortcode("Quote", inv =>
            {
                capturedPositional = inv.PositionalArgs;
                capturedNamed = inv.NamedArgs;
                return "ok";
            }),
        ]);

        await expander.ExpandAsync(
            """<?# Quote "a positional with spaces" title="A Long Title" /?>""",
            MakeContext(),
            TestContext.Current.CancellationToken);

        capturedPositional.ShouldNotBeNull();
        capturedPositional!.ShouldBe(["a positional with spaces"]);
        capturedNamed!["title"].ShouldBe("A Long Title");
    }

    [Fact]
    public async Task ExpandAsync_NameMatchIsCaseInsensitive()
    {
        var expander = new ShortcodeExpander([new CapturingShortcode("Greet", _ => "hi")]);

        var result = await expander.ExpandAsync(
            "<?# GREET /?> and <?# greet /?>",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldBe("hi and hi");
    }

    [Fact]
    public async Task ExpandAsync_InsideFencedCodeBlock_StillExpands()
    {
        // Expand everywhere — install snippets and code samples want the real value.
        // Authors who need a literal directive in a fence escape with HTML entities.
        var expander = new ShortcodeExpander([new CapturingShortcode("Greet", _ => "EXPANDED")]);
        var markdown = """
            Outer <?# Greet /?>.

            ```bash
            echo <?# Greet /?>
            ```

            After.
            """;

        var result = await expander.ExpandAsync(markdown, MakeContext(), TestContext.Current.CancellationToken);

        result.ShouldContain("Outer EXPANDED.");
        result.ShouldContain("echo EXPANDED");
        result.ShouldNotContain("<?# Greet /?>");
    }

    [Fact]
    public async Task ExpandAsync_UnknownShortcode_EmitsCommentFallback()
    {
        var expander = new ShortcodeExpander([new CapturingShortcode("Known", _ => "ok")]);

        var result = await expander.ExpandAsync(
            "<?# Mystery /?>",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldBe("<!-- Pennington: unknown shortcode 'Mystery' -->");
    }

    [Fact]
    public async Task ExpandAsync_UnclosedBlockShortcode_EmitsCommentFallback()
    {
        var expander = new ShortcodeExpander([new CapturingShortcode("Wrap", inv => $"[{inv.Content}]")]);

        var result = await expander.ExpandAsync(
            "Before <?# Wrap ?>content with no closer.",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldContain("<!-- Pennington: unterminated shortcode 'Wrap' -->");
        // Inline content remains in the source after the fallback comment is inserted.
        result.ShouldContain("content with no closer.");
    }

    [Fact]
    public async Task ExpandAsync_HandlerThrows_EmitsWarningCommentAndContinues()
    {
        var expander = new ShortcodeExpander([
            new CapturingShortcode("Boom", _ => throw new InvalidOperationException("nope")),
            new CapturingShortcode("Good", _ => "ok"),
        ]);

        var result = await expander.ExpandAsync(
            "<?# Boom /?> then <?# Good /?>",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldContain("<!-- Pennington: shortcode 'Boom' failed: nope -->");
        result.ShouldContain("then ok");
    }

    [Fact]
    public async Task ExpandAsync_BackslashEscape_DropsSlashAndEmitsLiteralOpener()
    {
        var expander = new ShortcodeExpander([new CapturingShortcode("Version", _ => "9.9.9")]);

        var result = await expander.ExpandAsync(
            @"Real: <?# Version /?>. Literal: \<?# Version /?>.",
            MakeContext(),
            TestContext.Current.CancellationToken);

        // Real call expands; escaped call survives the expander with the backslash stripped.
        result.ShouldBe("Real: 9.9.9. Literal: <?# Version /?>.");
    }

    [Fact]
    public async Task ExpandAsync_OrphanCloser_EmitsCommentFallback()
    {
        var expander = new ShortcodeExpander([new CapturingShortcode("Foo", _ => "ok")]);

        var result = await expander.ExpandAsync(
            "Before <?#/ Foo ?> after.",
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldContain("<!-- Pennington: orphan shortcode closer -->");
        result.ShouldContain("Before ");
        result.ShouldContain(" after.");
    }
}

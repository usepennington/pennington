using MonorailCss;
using MonorailCss.Theme;
using Pennington.MonorailCss;

namespace Pennington.Tests.MonorailCss;

public class ColorThemeTests
{
    private static string ProcessWithTheme(ColorTheme theme, params string[] classes)
    {
        var built = theme.ApplyToTheme(Theme.CreateWithDefaults());
        var framework = new CssFramework(new CssFrameworkSettings { Theme = built });
        return framework.Process(classes);
    }

    [Fact]
    public void All_HasTwelveDistinctNamedThemes()
    {
        ColorTheme.All.Count.ShouldBe(12);
        ColorTheme.All.Select(t => t.Name).Distinct().Count().ShouldBe(12);
        ColorTheme.All.ShouldAllBe(t => t.PrimaryHue >= 0 && t.PrimaryHue < 360);
    }

    [Fact]
    public void ApplyToTheme_RegistersBrandAndSyntaxPalettesAsOklch()
    {
        // The custom hyphenated syntax-* palettes (and the brand roles) must resolve into real
        // oklch() utilities — this guards the multi-segment palette-name resolution the
        // .hljs-* @apply rules depend on.
        var css = ProcessWithTheme(
            ColorTheme.Ember,
            "text-primary-500", "bg-base-500", "text-accent-500",
            "text-syntax-keyword-800", "text-syntax-string-800",
            "text-syntax-variable-800", "text-syntax-function-800");

        css.ShouldContain(".text-syntax-variable-800");
        css.ShouldContain(".text-syntax-function-800");
        css.ShouldContain("oklch(");
    }

    [Fact]
    public void Base_AutoSelectsNearestNeutral_AndBaseColorNameOverridesIt()
    {
        // Default: base-500 resolves to the stock neutral whose undertone is nearest the seed hue.
        // Azure (hue 242) lands on slate.
        var auto = ColorVar(ProcessWithTheme(ColorTheme.Azure, "bg-base-500"), "base-500");
        auto.ShouldContain("slate");

        // Overridden: base-500 now resolves to the requested zinc palette instead.
        var overridden = ColorVar(ProcessWithTheme(ColorTheme.Azure with { BaseColorName = ColorName.Zinc }, "bg-base-500"), "base-500");
        overridden.ShouldContain("zinc");
        overridden.ShouldNotBe(auto);
    }

    [Theory]
    [InlineData(30, "taupe")]   // Ember — warm
    [InlineData(115, "olive")]  // Citron — yellow-green
    [InlineData(210, "mist")]   // Aqua — cyan
    [InlineData(242, "slate")]  // Azure — cool blue
    [InlineData(325, "mauve")]  // Orchid — magenta
    public void NeutralForHue_PicksNearestUndertone(double hue, string expected)
    {
        ColorTheme.NeutralForHue(hue).Value.ShouldBe(expected);
    }

    // Pulls the value out of a `--color-<key>: <value>;` custom-property definition.
    private static string ColorVar(string css, string key)
    {
        var marker = $"--color-{key}:";
        var start = css.IndexOf(marker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0, $"expected a --color-{key} definition");
        var valueStart = start + marker.Length;
        return css[valueStart..css.IndexOf(';', valueStart)].Trim();
    }

    [Fact]
    public void SyntaxTheme_SlotsResolveToRegisteredPalettes()
    {
        var theme = ColorTheme.Ember;
        var syntax = theme.SyntaxTheme;

        // Every slot the HljsApplies builder consumes must point at a palette that actually
        // resolves (comment tracks the neutral base; the rest track the syntax-* accents).
        var classes = new[]
        {
            $"text-{syntax.Keyword.Value}-800",
            $"text-{syntax.String.Value}-800",
            $"text-{syntax.Variable.Value}-800",
            $"text-{syntax.Function.Value}-800",
            $"text-{syntax.Comment.Value}-600",
        };

        var css = ProcessWithTheme(theme, classes);

        foreach (var c in classes)
        {
            css.ShouldContain("." + c);
        }
    }
}

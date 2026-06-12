using System.Reflection;
using Pennington.MonorailCss;
using Pennington.UI.Styling;

namespace Pennington.Tests.UI;

public class StyleRegistryTests
{
    // The override layer merges with the site template's MonorailCSS class merger. Tests build one
    // from default options (Blue/Purple/Slate semantic palette, scrollbar/card custom utilities) so
    // the conflict model matches what DocSite/BlogSite render with.
    private static readonly Func<string, string, string> Merger =
        MonorailCssService.CreateClassMerger(new MonorailCssOptions());

    [Fact]
    public void Get_ReturnsUiDefault_WhenNoSkinOrOverride()
    {
        var registry = StyleRegistry.Create();

        registry[StyleKeys.TocListGap].ShouldBe("gap-4");
        registry.Entries.ShouldAllBe(e => e.Source == StyleSource.Default);
    }

    [Fact]
    public void Get_ReturnsSkinVerbatim_WhenSkinProvided()
    {
        // A skin replaces the component default wholesale — no merge, so none of the
        // default's structure (border-l, pl-3.5) survives.
        var skin = new Dictionary<string, string>
        {
            [StyleKeys.TocLinkStructure] = "flex items-center px-2.5 py-1.5 rounded-md",
        };

        var registry = StyleRegistry.Create(skin);

        registry[StyleKeys.TocLinkStructure].ShouldBe("flex items-center px-2.5 py-1.5 rounded-md");
        registry.Entries.Single(e => e.Key == StyleKeys.TocLinkStructure)
            .Source.ShouldBe(StyleSource.TemplateSkin);
    }

    [Fact]
    public void Get_MergesOverrideOverSkin()
    {
        var skin = new Dictionary<string, string>
        {
            [StyleKeys.TocLinkColor] = "transition-colors duration-150 text-base-500 hover:bg-base-100",
        };
        var overrides = new Dictionary<string, string>
        {
            [StyleKeys.TocLinkColor] = "text-emerald-600",
        };

        var registry = StyleRegistry.Create(skin, overrides, Merger);

        var classes = registry[StyleKeys.TocLinkColor].Split(' ');
        classes.ShouldContain("text-emerald-600");
        classes.ShouldContain("hover:bg-base-100");
        classes.ShouldContain("transition-colors");
        classes.ShouldNotContain("text-base-500");
        registry.Entries.Single(e => e.Key == StyleKeys.TocLinkColor)
            .Source.ShouldBe(StyleSource.ConsumerOverride);
    }

    [Fact]
    public void Get_MergesOverrideOverUiDefault_WhenNoSkin()
    {
        var overrides = new Dictionary<string, string>
        {
            [StyleKeys.TocRootLinkColor] = "text-emerald-600",
        };

        var registry = StyleRegistry.Create(overrides: overrides, mergeOverride: Merger);

        var classes = registry[StyleKeys.TocRootLinkColor].Split(' ');
        classes.ShouldContain("text-emerald-600");
        classes.ShouldNotContain("text-base-700");
        classes.ShouldContain("dark:text-base-400");
    }

    [Fact]
    public void Create_Throws_OnUnknownOverrideKey_ListingValidKeys()
    {
        var overrides = new Dictionary<string, string> { ["toc.link-colour"] = "text-emerald-600" };

        var ex = Should.Throw<InvalidOperationException>(() => StyleRegistry.Create(overrides: overrides));

        ex.Message.ShouldContain("toc.link-colour");
        ex.Message.ShouldContain(StyleKeys.TocLinkColor);
    }

    [Fact]
    public void Create_Throws_OnUnknownSkinKey()
    {
        var skin = new Dictionary<string, string> { ["toc.bogus"] = "gap-2" };

        Should.Throw<InvalidOperationException>(() => StyleRegistry.Create(skin));
    }

    [Fact]
    public void Get_Throws_OnUnknownKey_ListingValidKeys()
    {
        var registry = StyleRegistry.Create();

        var ex = Should.Throw<InvalidOperationException>(() => registry["toc.nope"]);

        ex.Message.ShouldContain(StyleKeys.TocListGap);
    }

    [Fact]
    public void Keys_AreCaseInsensitive()
    {
        var overrides = new Dictionary<string, string> { ["TOC.List-Gap"] = "gap-2" };

        var registry = StyleRegistry.Create(overrides: overrides, mergeOverride: Merger);

        registry["TOC.LIST-GAP"].ShouldBe("gap-2");
        registry[StyleKeys.TocListGap].ShouldBe("gap-2");
    }

    [Fact]
    public void Get_MergesGapOverride()
    {
        // The MonorailCSS merger derives gap conflicts from compiled output natively, so a gap
        // override replaces the default instead of stacking next to it.
        var overrides = new Dictionary<string, string> { [StyleKeys.TocListGap] = "gap-2" };

        StyleRegistry.Create(overrides: overrides, mergeOverride: Merger)[StyleKeys.TocListGap]
            .ShouldBe("gap-2");
    }

    [Fact]
    public void Merger_DropsConflictingSemanticPaletteClass()
    {
        // The MonorailCSS semantic palette (base/primary/accent) compiles to real color
        // declarations, so text-base-500 conflicts with a consumer's text-emerald-600 and is
        // knocked out.
        Merger("text-base-500", "text-emerald-600").ShouldBe("text-emerald-600");
    }

    [Fact]
    public void Merger_KeepsDifferentModifiers()
    {
        // dark: variants don't conflict with bare utilities — consumers must override
        // dark: classes explicitly.
        var merged = Merger("text-base-500 dark:text-base-400", "text-emerald-600");

        merged.Split(' ').ShouldBe(["dark:text-base-400", "text-emerald-600"], ignoreOrder: true);
    }

    [Fact]
    public void Merger_KeepsNonConflictingCustomUtility()
    {
        // Pennington's scrollbar-* custom utilities write scrollbar properties, not color, so
        // they survive a text-color override untouched.
        Merger("scrollbar-thin text-base-500", "text-emerald-600")
            .Split(' ').ShouldContain("scrollbar-thin");
    }

    [Fact]
    public void StyleKeys_CatalogMatchesRegistryEntries()
    {
        var catalog = typeof(StyleKeys)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToHashSet();

        var registered = StyleRegistry.Create().Entries.Select(e => e.Key).ToHashSet();

        registered.ShouldBe(catalog, ignoreOrder: true);
    }
}

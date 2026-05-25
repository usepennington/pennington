namespace Pennington.TreeSitter.Tests;

using Pennington.TreeSitter.Fragments;
using Pennington.TreeSitter.Resolution;
using TsLanguage = global::TreeSitter.Language;
using TsParser = global::TreeSitter.Parser;

/// <summary>Helpers for parsing inline source with the bundled grammars and exercising the resolver/extractor.</summary>
internal static class TreeSitterTestHelper
{
    public static LanguageDeclarationConfig Config(string languageKey) =>
        LanguageDeclarationConfigDefaults.CreateDefaults()[languageKey];

    /// <summary>Resolves and extracts a member from inline source, returning null when the path does not resolve.</summary>
    public static string? Extract(string languageKey, string source, string namePath, FragmentOptions? options = null)
    {
        var config = Config(languageKey);
        using var language = new TsLanguage(config.TreeSitterLanguageName);
        using var parser = new TsParser(language);
        using var tree = parser.Parse(source)!;

        var segments = namePath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var node = new NamePathResolver().Resolve(tree.RootNode, segments, config);
        return node is null ? null : FragmentExtractor.Extract(node, config, options ?? FragmentOptions.Default);
    }

    /// <summary>Returns the tree-sitter s-expression for inline source — useful for inspecting real node types.</summary>
    public static string Dump(string treeSitterLanguageName, string source)
    {
        using var language = new TsLanguage(treeSitterLanguageName);
        using var parser = new TsParser(language);
        using var tree = parser.Parse(source)!;
        return tree.RootNode.Expression;
    }
}

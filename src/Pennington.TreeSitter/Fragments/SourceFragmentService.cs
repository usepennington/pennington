namespace Pennington.TreeSitter.Fragments;

using Parsing;
using Resolution;

/// <summary>Default <see cref="ISourceFragmentService"/>: reads a file, parses it with tree-sitter, and extracts the named fragment.</summary>
internal sealed class SourceFragmentService : ISourceFragmentService
{
    private readonly TreeSitterOptions _options;
    private readonly ITreeSitterParserPool _pool;
    private readonly NamePathResolver _resolver;

    public SourceFragmentService(TreeSitterOptions options, ITreeSitterParserPool pool, NamePathResolver resolver)
    {
        _options = options;
        _pool = pool;
        _resolver = resolver;
    }

    public FragmentResult GetFragment(string languageId, string relativeFilePath, string namePath, FragmentOptions options)
    {
        if (string.IsNullOrEmpty(_options.ContentRoot))
        {
            return FragmentResult.Fail("Tree-sitter ContentRoot is not configured.");
        }

        if (relativeFilePath.Contains("..") || Path.IsPathRooted(relativeFilePath))
        {
            return FragmentResult.Fail($"Invalid file path: {relativeFilePath}");
        }

        var fullPath = Path.Combine(Path.GetFullPath(_options.ContentRoot), relativeFilePath);
        if (!File.Exists(fullPath))
        {
            return FragmentResult.Fail($"File not found: {relativeFilePath}");
        }

        var source = File.ReadAllText(fullPath);

        // A bare file reference (no member path) embeds the whole file and needs no language config.
        var segments = namePath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return FragmentResult.Ok(source);
        }

        var config = _options.ResolveConfig(languageId);
        if (config is null)
        {
            return FragmentResult.Fail($"No tree-sitter language configured for '{languageId}'.");
        }

        using var lease = _pool.Rent(config.TreeSitterLanguageName);
        using var tree = lease.Parser.Parse(source);
        if (tree is null)
        {
            return FragmentResult.Fail($"Failed to parse {relativeFilePath}.");
        }

        var node = _resolver.Resolve(tree.RootNode, segments, config);
        if (node is null)
        {
            return FragmentResult.Fail($"Member '{namePath}' not found in {relativeFilePath}.");
        }

        var fragment = FragmentExtractor.Extract(node, config, options);
        if (options.IncludeImports)
        {
            var imports = ImportCollector.Collect(tree.RootNode, config);
            if (imports.Length > 0)
            {
                fragment = imports + "\n\n" + fragment;
            }
        }

        return FragmentResult.Ok(fragment);
    }
}

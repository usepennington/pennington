namespace Pennington.Highlighting;

using System.Net;
using System.Text;
using Infrastructure;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;

/// <summary>
/// Provides syntax highlighting for code blocks using TextMate grammars.
/// Implements ICodeHighlighter with priority 50.
/// </summary>
public sealed class TextMateHighlighter : ICodeHighlighter
{
    private readonly TextMateLanguageRegistry _languageRegistry;
    private readonly Registry _registry;
    private static readonly List<(string Scope, string CssClass)> ScopeMappings;
    private static readonly TimeSpan TokenizeTimeLimit = TimeSpan.FromSeconds(5);
    private static readonly Lock RegistryAccessLock = new();

    static TextMateHighlighter()
    {
        ScopeMappings =
        [
            // Comments
            ("comment.line.double-slash", "hljs-comment"),
            ("comment.block.documentation", "hljs-comment"),
            ("comment.block", "hljs-comment"),
            ("comment", "hljs-comment"),
            ("punctuation.definition.comment", "hljs-comment"),

            // Entities (functions, types, tags, attributes)
            ("entity.name.function", "hljs-title"),
            ("entity.name.type", "hljs-type"),
            ("entity.name.class", "hljs-type"),
            ("entity.name.interface", "hljs-type"),
            ("entity.name.struct", "hljs-type"),
            ("entity.name.enum", "hljs-type"),
            ("entity.name.tag", "hljs-tag"),
            ("entity.other.attribute-name", "hljs-attr"),
            ("entity.other.inherited-class", "hljs-type"),
            ("meta.attribute.cs", "hljs-meta"),

            // Keywords
            ("keyword.control", "hljs-keyword"),
            ("keyword.operator.new", "hljs-keyword"),
            ("keyword.operator", "hljs-operator"),
            ("keyword", "hljs-keyword"),

            // Storage (type keywords, modifiers)
            ("storage.type", "hljs-keyword"),
            ("storage.modifier", "hljs-keyword"),

            // Constants and Literals
            ("constant.numeric", "hljs-number"),
            ("constant.language", "hljs-literal"),
            ("constant.character.escape", "hljs-regexp"),
            ("constant.other", "hljs-literal"),

            // Strings
            ("string.quoted.interpolated", "hljs-string"),
            ("string.regexp", "hljs-regexp"),
            ("string", "hljs-string"),
            ("punctuation.definition.string", "hljs-string"),

            // Punctuation
            ("punctuation.definition.tag", "hljs-tag"),
            ("punctuation.separator", "hljs-punctuation"),
            ("punctuation.terminator", "hljs-punctuation"),
            ("punctuation.accessor", "hljs-punctuation"),
            ("punctuation.section.embedded.begin", "hljs-punctuation"),
            ("punctuation.section.embedded.end", "hljs-punctuation"),
            ("punctuation", "hljs-punctuation"),

            // Variables
            ("variable.parameter", "hljs-variable"),
            ("variable.language", "hljs-variable"),
            ("variable.other.member", "hljs-attr"),
            ("variable.other.object.property", "hljs-attr"),
            ("variable.other.constant", "hljs-literal"),
            ("variable.other.enummember", "hljs-attr"),
            ("variable", "hljs-variable"),

            // Support (built-in functions, classes, constants)
            ("support.function", "hljs-built_in"),
            ("support.class", "hljs-type"),
            ("support.type", "hljs-keyword"),
            ("support.constant", "hljs-literal"),

            // Markup
            ("markup.inserted", "hljs-addition"),
            ("markup.deleted", "hljs-deletion"),

            // Meta scopes
            ("meta.selector", "hljs-selector-tag"),
            ("meta.tag", "hljs-tag"),
            ("meta.definition.method", "hljs-function"),
            ("meta.definition.type", "hljs-type"),

            // Fallback
            ("entity", "hljs-name"),
        ];
    }

    public TextMateHighlighter(TextMateLanguageRegistry languageRegistry)
    {
        _languageRegistry = languageRegistry ?? throw new ArgumentNullException(nameof(languageRegistry));
        _registry = languageRegistry.Registry;
    }

    public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "*" };

    public int Priority => 50;

    public string Highlight(string code, string language)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        lock (RegistryAccessLock)
        {
            var grammar = ResolveGrammar(language);

            if (grammar == null)
            {
                var escapedCode = WebUtility.HtmlEncode(code);
                return $"<pre><code class=\"language-{language} code\">{escapedCode}</code></pre>";
            }

            return TokenizeAndRender(code, grammar);
        }
    }

    private IGrammar? ResolveGrammar(string language)
    {
        var scopeName = _languageRegistry.GetScopeNameForLanguage(language);
        IGrammar? grammar = null;

        if (!string.IsNullOrEmpty(scopeName))
        {
            grammar = _registry.LoadGrammar(scopeName);
        }

        if (grammar == null)
        {
            var potentialScopeNames = new[]
            {
                $"source.{language.ToLowerInvariant()}",
                language.ToLowerInvariant(),
            };

            foreach (var potentialScope in potentialScopeNames)
            {
                try
                {
                    grammar = _registry.LoadGrammar(potentialScope);
                    if (grammar != null) break;
                }
                catch
                {
                    // Ignore exceptions if a potential scope name is invalid
                }
            }
        }

        return grammar;
    }

    private static string TokenizeAndRender(string code, IGrammar grammar)
    {
        var sb = new StringBuilder();
        sb.Append("<pre><code>");

        var lines = code.SplitNewLines();
        IStateStack? ruleStack = null;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var result = grammar.TokenizeLine(line, ruleStack, TokenizeTimeLimit);
            ruleStack = result.RuleStack;

            var currentIndex = 0;
            foreach (var token in result.Tokens)
            {
                if (token.StartIndex > currentIndex)
                {
                    sb.Append(WebUtility.HtmlEncode(line.Substring(currentIndex, token.StartIndex - currentIndex)));
                }

                var length = token.Length;
                if (token.StartIndex + length > line.Length)
                {
                    length = line.Length - token.StartIndex;
                }

                var tokenText = line.Substring(token.StartIndex, length);
                var escapedTokenText = WebUtility.HtmlEncode(tokenText);
                var hljsClass = GetHljsClassForScopes(token.Scopes);

                if (!string.IsNullOrEmpty(hljsClass))
                {
                    sb.Append($"<span class=\"{hljsClass}\">{escapedTokenText}</span>");
                }
                else
                {
                    sb.Append(escapedTokenText);
                }

                currentIndex = token.StartIndex + length;
            }

            if (i < lines.Length - 1)
            {
                sb.AppendLine();
            }
        }

        sb.Append("</code></pre>");
        return sb.ToString();
    }

    private static string? GetHljsClassForScopes(List<string> scopes)
    {
        if (scopes.Count == 0)
            return null;

        for (var i = scopes.Count - 1; i >= 0; i--)
        {
            var currentScope = scopes[i];
            foreach (var (scope, cssClass) in ScopeMappings)
            {
                if (currentScope.StartsWith(scope))
                    return cssClass;
            }
        }

        return null;
    }
}
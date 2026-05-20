namespace Pennington.Markdown.Shortcodes;

using System.Text;
using Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Expands Statiq-style pre-render shortcodes in a markdown source.
/// Recognises <c>&lt;?# Name args ?&gt;...&lt;?#/ Name ?&gt;</c> blocks and
/// <c>&lt;?# Name args /?&gt;</c> self-closing tags, dispatches each call to a
/// matching <see cref="IShortcode"/> registered in DI, and splices the handler's
/// output back into the source before Markdig parses it. Directives expand
/// everywhere — including inside fenced code blocks — so install snippets and
/// generated samples can stamp real values. To show a literal directive without
/// expanding it, prefix the opener with a backslash (<c>\&lt;?# ... ?&gt;</c>);
/// the expander consumes the backslash and emits the directive as-is. Unknown
/// names and handler failures degrade to HTML comments and a diagnostic so one
/// bad call site cannot fail the render.
/// </summary>
public sealed class ShortcodeExpander
{
    private readonly Dictionary<string, IShortcode> _shortcodes;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>Creates the expander with the DI-registered handlers and an optional accessor for per-request <see cref="DiagnosticContext"/>.</summary>
    public ShortcodeExpander(
        IEnumerable<IShortcode>? shortcodes = null,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _shortcodes = (shortcodes ?? [])
            .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);
        _httpContextAccessor = httpContextAccessor;
    }

    private DiagnosticContext? Diagnostics
        => _httpContextAccessor?.HttpContext?.RequestServices.GetService<DiagnosticContext>();

    /// <summary>
    /// Returns <paramref name="markdown"/> with every shortcode call site replaced by its handler
    /// output. Returns the input unchanged when no handlers are registered or the source contains
    /// no shortcode opener.
    /// </summary>
    public async Task<string> ExpandAsync(
        string markdown,
        ShortcodeContext context,
        CancellationToken cancellationToken)
    {
        if (_shortcodes.Count == 0 || !markdown.Contains("<?#", StringComparison.Ordinal))
        {
            return markdown;
        }

        var result = new StringBuilder(markdown.Length);
        var position = 0;

        while (position < markdown.Length)
        {
            var openerStart = markdown.IndexOf("<?#", position, StringComparison.Ordinal);
            if (openerStart < 0)
            {
                result.Append(markdown, position, markdown.Length - position);
                break;
            }

            result.Append(markdown, position, openerStart - position);

            // Backslash escape: \<?# is a literal opener, not a call site. Drop the
            // backslash and emit the opener as-is — Markdig HTML-encodes it downstream
            // so the reader sees "<?#" in their rendered code samples and prose.
            if (openerStart > 0 && markdown[openerStart - 1] == '\\')
            {
                result.Length -= 1;
                result.Append("<?#");
                position = openerStart + 3;
                continue;
            }

            // Distinguish opener "<?# " from orphan closer "<?#/".
            var afterPrefix = openerStart + 3;
            if (afterPrefix < markdown.Length && markdown[afterPrefix] == '/')
            {
                var orphanEnd = markdown.IndexOf("?>", afterPrefix, StringComparison.Ordinal);
                if (orphanEnd < 0)
                {
                    Diagnostics?.AddWarning("Unterminated shortcode closer.");
                    result.Append("<!-- Pennington: unterminated shortcode closer -->");
                    position = markdown.Length;
                    break;
                }

                Diagnostics?.AddWarning("Orphan shortcode closer (no matching opener).");
                result.Append("<!-- Pennington: orphan shortcode closer -->");
                position = orphanEnd + 2;
                continue;
            }

            var openerEnd = markdown.IndexOf("?>", afterPrefix, StringComparison.Ordinal);
            if (openerEnd < 0)
            {
                Diagnostics?.AddWarning("Unterminated shortcode opener.");
                result.Append("<!-- Pennington: unterminated shortcode opener -->");
                position = markdown.Length;
                break;
            }

            var openerBody = markdown[afterPrefix..openerEnd].Trim();
            var isSelfClosing = openerBody.EndsWith('/');
            if (isSelfClosing)
            {
                openerBody = openerBody[..^1].TrimEnd();
            }

            var parsed = ParseNameAndArgs(openerBody);
            if (parsed is null)
            {
                Diagnostics?.AddWarning("Malformed shortcode opener.");
                result.Append("<!-- Pennington: malformed shortcode -->");
                position = openerEnd + 2;
                continue;
            }

            var (name, positional, named) = parsed.Value;
            string? content = null;
            int endPosition;

            if (isSelfClosing)
            {
                endPosition = openerEnd + 2;
            }
            else
            {
                var closer = FindCloser(markdown, openerEnd + 2, name);
                if (closer is null)
                {
                    Diagnostics?.AddWarning($"Unterminated shortcode '{name}' (no matching closer).");
                    result.Append($"<!-- Pennington: unterminated shortcode '{name}' -->");
                    position = openerEnd + 2;
                    continue;
                }

                content = markdown[(openerEnd + 2)..closer.Value.ContentEnd];
                endPosition = closer.Value.CloserEnd;
            }

            if (!_shortcodes.TryGetValue(name, out var handler))
            {
                Diagnostics?.AddWarning($"Unknown shortcode '{name}'.");
                result.Append($"<!-- Pennington: unknown shortcode '{name}' -->");
                position = endPosition;
                continue;
            }

            string output;
            try
            {
                var invocation = new ShortcodeInvocation(positional, named, content);
                output = await handler.ExecuteAsync(invocation, context, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Handler exceptions degrade to a warning + HTML comment so one bad call
                // site never fails the build. Devs who want hard failure can flip the
                // diagnostic to error in their own response processor.
                Diagnostics?.AddWarning($"Shortcode '{name}' failed: {ex.Message}");
                output = $"<!-- Pennington: shortcode '{name}' failed: {ex.Message} -->";
            }

            result.Append(output);
            position = endPosition;
        }

        return result.ToString();
    }

    /// <summary>Finds <c>&lt;?#/ Name ?&gt;</c> starting at <paramref name="from"/>; returns its content-end and closer-end offsets.</summary>
    private static (int ContentEnd, int CloserEnd)? FindCloser(string source, int from, string name)
    {
        var search = from;
        while (search < source.Length)
        {
            var candidate = source.IndexOf("<?#/", search, StringComparison.Ordinal);
            if (candidate < 0)
            {
                return null;
            }

            var closerEnd = source.IndexOf("?>", candidate + 4, StringComparison.Ordinal);
            if (closerEnd < 0)
            {
                return null;
            }

            var body = source[(candidate + 4)..closerEnd].Trim();
            if (string.Equals(body, name, StringComparison.OrdinalIgnoreCase))
            {
                return (candidate, closerEnd + 2);
            }

            search = closerEnd + 2;
        }

        return null;
    }

    /// <summary>Splits an opener body into <c>(name, positional, named)</c>; returns null when the name is missing or not a valid identifier.</summary>
    private static (string Name, IReadOnlyList<string> Positional, IReadOnlyDictionary<string, string> Named)? ParseNameAndArgs(string body)
    {
        if (body.Length == 0)
        {
            return null;
        }

        var tokens = Tokenize(body);
        if (tokens.Count == 0)
        {
            return null;
        }

        var name = tokens[0];
        if (!IsValidName(name))
        {
            return null;
        }

        var positional = new List<string>();
        var named = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var eq = token.IndexOf('=');
            if (eq > 0 && IsValidName(token[..eq]))
            {
                named[token[..eq]] = Unquote(token[(eq + 1)..]);
            }
            else
            {
                positional.Add(Unquote(token));
            }
        }

        return (name, positional, named);
    }

    /// <summary>Splits the opener body into whitespace-separated tokens, honouring double-quoted runs.</summary>
    private static List<string> Tokenize(string body)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var inQuote = false;

        for (var i = 0; i < body.Length; i++)
        {
            var c = body[i];

            if (inQuote)
            {
                if (c == '\\' && i + 1 < body.Length && body[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                if (c == '"')
                {
                    inQuote = false;
                    continue;
                }

                current.Append(c);
                continue;
            }

            if (c == '"')
            {
                inQuote = true;
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }

    private static bool IsValidName(string s)
    {
        if (s.Length == 0)
        {
            return false;
        }

        if (!char.IsLetter(s[0]) && s[0] != '_')
        {
            return false;
        }

        for (var i = 1; i < s.Length; i++)
        {
            if (!char.IsLetterOrDigit(s[i]) && s[i] != '_' && s[i] != '-')
            {
                return false;
            }
        }

        return true;
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        {
            return value[1..^1].Replace("\\\"", "\"", StringComparison.Ordinal);
        }

        return value;
    }
}

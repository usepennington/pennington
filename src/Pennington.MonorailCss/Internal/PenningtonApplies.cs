using System.Collections.Immutable;
using MonorailCss;
using MonorailCss.Parser.Custom;

namespace Pennington.MonorailCss.Internal;

/// <summary>
/// Default <c>@apply</c> blocks and custom utilities Pennington layers on top of the MonorailCSS
/// framework. Authored as static dictionaries so the literal selectors and class strings are
/// visible to source review and to the discovery pipeline's IL scan.
/// </summary>
internal static class PenningtonApplies
{
    /// <summary>
    /// Returns every <c>@apply</c> block Pennington registers, parameterised by the
    /// <see cref="SyntaxTheme"/> in effect (only the <c>.hljs-*</c> rules vary).
    /// </summary>
    public static ImmutableDictionary<string, string> All(SyntaxTheme syntax) =>
        ImmutableDictionary<string, string>.Empty
            .AddRange(CodeBlockApplies)
            .AddRange(TabApplies)
            .AddRange(MarkdownAlertApplies)
            .AddRange(HljsApplies(syntax))
            .AddRange(SearchModalApplies);

    /// <summary>
    /// Custom utilities (scrollbar styling) registered with the MonorailCSS framework.
    /// </summary>
    public static readonly ImmutableList<UtilityDefinition> ScrollbarUtilities = ImmutableList.Create(
        new UtilityDefinition
        {
            Pattern = "scrollbar-thin",
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-width", "thin")),
        },
        new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)")),
        },
        new UtilityDefinition
        {
            Pattern = "scrollbar-track-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-track-color", "--value(--color-*)")),
        },
        new UtilityDefinition
        {
            Pattern = "scrollbar-track-transparent",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-track-color", "transparent")),
        },
        new UtilityDefinition
        {
            Pattern = "scrollbar-color",
            Declarations = ImmutableList.Create(
                new CssDeclaration(
                    "scrollbar-color",
                    "var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)")),
        });

    private static readonly ImmutableDictionary<string, string> CodeBlockApplies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            {
                ".code-highlight-wrapper .standalone-code-container",
                "bg-white/50 border border-base-300/75 shadow-xs rounded rounded-xl overflow-x-auto dark:bg-black/20 dark:border-base-700/50"
            },
            {
                ".code-highlight-wrapper pre ",
                "py-2 md:py-3 px-4 md:px-8  overflow-x-auto  font-mono text-xs md:text-sm  leading-relaxed w-full dark:scheme-dark"
            },
            {
                ".code-highlight-wrapper .standalone-code-highlight pre",
                "text-base-900/90 dark:text-base-100/90"
            },
            {
                ".code-highlight-wrapper pre code",
                "font-mono inline-block min-w-full"
            },

            // Code transformation line containers
            {
                ".code-highlight-wrapper .line",
                "inline-block transition-all duration-300 lg:py-[1px] px-8 -mx-8 w-[calc(100%+4rem)] relative"
            },
            {
                ".code-highlight-wrapper pre.has-focused .line",
                "blur-[0.05rem] opacity-75"
            },
            {
                ".code-highlight-wrapper pre.has-focused:hover .line",
                "blur-[0] opacity-100"
            },

            // Line highlighting
            {
                ".code-highlight-wrapper .line.highlight",
                "bg-primary-700/20 dark:bg-primary-500/20"
            },

            // Diff notation
            {
                ".code-highlight-wrapper .line.diff-add",
                "bg-emerald-600/20 dark:bg-emerald-900/20 before:font-bold before:content-['+'] before:hidden md:before:block before:text-sm before:absolute before:left-1 before:green:text-green-500 before:text-green-700"
            },
            {
                ".code-highlight-wrapper .line.diff-remove",
                "bg-red-600/20 dark:bg-red-900/20 before:font-bold  before:content-['-'] before:hidden md:before:block before:text-sm before:absolute before:left-1 before:dark:text-red-500 before:text-red-700"
            },
            {
                ".code-highlight-wrapper .line.diff-remove > *",
                "opacity-50 contrast-50"
            },

            // Focus and blur
            {
                ".code-highlight-wrapper pre.has-focused  .line.focused",
                "blur-[0] opacity-100"
            },

            // Error and warning states
            {
                ".code-highlight-wrapper .line.error",
                "bg-red-300/50 dark:bg-red-500/20"
            },
            {
                ".code-highlight-wrapper .line.warning",
                "bg-amber-300/50 dark:bg-amber-400/20"
            },

            // Word highlighting
            {
                ".code-highlight-wrapper .word-highlight",
                "border border-primary-600 dark:border-primary-300/25 rounded px-0.5 py-0 bg-primary-100/25 dark:bg-primary-500/10"
            },
            {
                ".code-highlight-wrapper .word-highlight-with-message",
                "border border-b border-primary-600 dark:border-primary-300/25 rounded px-1 py-1 bg-primary-100/25 dark:bg-primary-500/10 relative "
            },
            {
                ".code-highlight-wrapper .word-highlight-wrapper",
                "relative inline-block"
            },
            {
                ".code-highlight-wrapper .word-highlight-message",
                "font-sans font-semilight tracking-loose absolute top-full left-0 mt-3 px-2 py-1 text-xs text-base-800 bg-base-200/25 dark:bg-primary-700/20 dark:text-primary-200 rounded border border-primary-500/50 whitespace-nowrap z-10 select-none pointer-events-none"
            },
            {
                ".code-highlight-wrapper .word-highlight-message::selection",
                "bg-transparent"
            },
            {
                ".code-highlight-wrapper .line:has(.word-highlight-wrapper)",
                "mb-12"
            },
        });

    private static readonly ImmutableDictionary<string, string> TabApplies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            {
                ".tab-container",
                "flex flex-col bg-base-100 border border-base-300/75 shadow-xs rounded rounded-xl overflow-x-auto dark:bg-base-950/25 dark:border-base-700/50"
            },
            {
                ".tab-list",
                "flex flex-row flex-wrap px-4 pt-1 bg-base-200/90 gap-x-2 lg:gap-x-4 dark:bg-base-800/50"
            },
            {
                ".tab-button",
                "whitespace-nowrap border-b border-transparent py-2 text-xs text-base-900/90 font-medium transition-colors hover:text-accent-500 disabled:pointer-events-none disabled:opacity-50 data-[selected=true]:text-accent-700 data-[selected=true]:border-accent-700 dark:text-base-100/90 dark:hover:text-accent-300 dark:data-[selected=true]:text-accent-400 dark:data-[selected=true]:border-accent-400"
            },
            {
                ".tab-panel",
                "hidden data-[selected=true]:block py-3 "
            },
        });

    private static readonly ImmutableDictionary<string, string> MarkdownAlertApplies =
        BuildMarkdownAlertApplies();

    private static ImmutableDictionary<string, string> BuildMarkdownAlertApplies()
    {
        const string alertFormatString =
            "fill-{0}-700 dark:fill-{0}-500 bg-{0}-100/75 border-{0}-500/20 dark:border-{0}-500/30 dark:bg-{0}-900/25 text-{0}-800 dark:text-{0}-200";

        return ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            { ".markdown-alert", "my-6 px-4 flex flex-row gap-2.5 rounded-2xl border text-sm items-center" },
            { ".markdown-alert a", "underline" },
            { ".markdown-alert-note", string.Format(alertFormatString, "emerald") },
            { ".markdown-alert-tip", string.Format(alertFormatString, "blue") },
            { ".markdown-alert-caution", string.Format(alertFormatString, "amber") },
            { ".markdown-alert-warning", string.Format(alertFormatString, "rose") },
            { ".markdown-alert-important", string.Format(alertFormatString, "sky") },
            { ".markdown-alert-title", "text-[0px]" },
            { ".markdown-alert svg", "h-4 w-4 mt-0.5" },
        });
    }

    private static ImmutableDictionary<string, string> HljsApplies(SyntaxTheme syntax)
    {
        string Token(ColorName c) => $"text-{c.Value}-800 dark:text-{c.Value}-300";
        string Soft(ColorName c) => $"text-{c.Value}-700 dark:text-{c.Value}-300";

        var keyword = Token(syntax.Keyword);
        var @string = Token(syntax.String);
        var variable = Token(syntax.Variable);
        var function = Token(syntax.Function);

        return ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            { ".hljs", "text-base-900 dark:text-base-200" },

            // Comments
            { ".hljs-comment", $"text-{syntax.Comment.Value}-600 italic dark:text-{syntax.Comment.Value}-400" },
            { ".hljs-quote", $"text-{syntax.Comment.Value}-800/50 italic dark:text-{syntax.Comment.Value}-300" },

            // Keywords and control flow
            { ".hljs-keyword", keyword },
            { ".hljs-selector-tag", Soft(syntax.Keyword) },
            { ".hljs-literal", keyword },
            { ".hljs-type", "text-base-700 dark:text-base-300" },

            // Strings and characters
            { ".hljs-string", @string },
            { ".hljs-number", @string },
            { ".hljs-regexp", @string },

            // Functions and methods
            { ".hljs-function", function },
            { ".hljs-title", function },
            { ".hljs-params", function },

            // Variables and identifiers
            { ".hljs-variable", variable },
            { ".hljs-name", variable },
            { ".hljs-attr", variable },
            { ".hljs-symbol", variable },

            // Operators and punctuation
            { ".hljs-operator", "text-base-800 dark:text-base-300" },
            { ".hljs-punctuation", "text-base-800 dark:text-base-300" },

            // Special elements
            { ".hljs-built_in", Soft(syntax.Function) },
            { ".hljs-class", keyword },
            { ".hljs-meta", "text-base-800 dark:text-base-300" },
            { ".hljs-tag", keyword },
            { ".hljs-attribute", variable },
            { ".hljs-addition", "text-green-800 dark:text-green-300" },
            { ".hljs-deletion", "text-red-800 dark:text-red-300" },
            { ".hljs-link", "text-blue-800 dark:text-blue-300" },
        });
    }

    private static readonly ImmutableDictionary<string, string> SearchModalApplies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            // Modal backdrop and container
            { ".search-modal-backdrop", "fixed inset-0 bg-base-950/50 backdrop-blur z-50 p-4 md:p-16" },
            {
                ".search-modal-content",
                " top-16 mx-auto w-full mt-8 max-w-2xl bg-base-100 dark:bg-base-900 rounded-lg shadow-xl border border-base-200 dark:border-base-700"
            },

            // Modal header and input
            { ".search-modal-header", "p-4 border-b border-base-200 dark:border-base-700" },
            { ".search-modal-input-container", "relative" },
            {
                ".search-modal-input",
                "w-full px-4 py-2 pl-10 bg-base-50 dark:bg-base-800 border border-base-300 dark:border-base-600 rounded-md text-base-900 dark:text-base-100 placeholder-base-500 dark:placeholder-base-400 focus:outline-none focus:ring-1 focus:ring-primary-500/50 focus:border-primary-500"
            },
            { ".search-modal-icon", "absolute left-3 top-2.5 h-4 w-4 text-base-400 dark:text-base-500" },

            // Results container
            { ".search-modal-results", "max-h-96 overflow-y-auto px-4 dark:scheme-dark" },

            // Status messages
            { ".search-modal-placeholder", "text-center text-base-600 dark:text-base-400 py-4" },
            { ".search-modal-loading", "text-center text-base-600 dark:text-base-400 py-4" },
            { ".search-modal-no-results", "text-center text-base-600 dark:text-base-400 py-4" },
            { ".search-modal-error", "text-center text-red-600 dark:text-red-400 py-4" },

            // Search result items
            { ".search-result-item", "border-b border-base-200 dark:border-base-800 py-4 last:border-b-0" },
            {
                ".search-result-link",
                "block hover:bg-base-50 dark:hover:bg-base-800 rounded-md p-2 -m-2 transition-colors"
            },
            { ".search-result-header", "flex items-start justify-between mb-1" },
            { ".search-result-title", "text-sm font-medium text-primary-700 dark:text-primary-400 flex-1" },
            { ".search-result-score", "text-xs text-base-500 dark:text-base-500 ml-2" },
            { ".search-result-description", "text-sm text-base-600 dark:text-base-400 mb-2" },
            { ".search-result-snippet", "text-xs text-base-700 dark:text-base-500" },
            { ".search-result-url", "text-xs text-base-500 dark:text-base-500 mt-2" },

            // Search highlighting
            { ".search-result-title .search-highlight", "text-primary-500 dark:text-primary-100 bg-inherit" },
            { ".search-highlight", "text-base-500 dark:text-base-50 bg-inherit" },
        });
}

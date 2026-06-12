namespace Pennington.Book;

using System.Collections.Immutable;
using System.CommandLine;
using Cli;
using Content;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Navigation;

/// <summary><c>diag books</c> — per book: title, slug, output path, and an ASCII chapters→pages tree. Read-only; never touches Chromium.</summary>
internal sealed class DiagBooksCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "books";

    /// <inheritdoc/>
    public string Description => "Print each PDF book's title, slug, output path, and its chapters→pages tree.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var localeOption = new Option<string>("--locale")
        {
            Description = "Locale to render (default: the site default locale).",
        };

        var command = new Command(Name, Description);
        command.Options.Add(localeOption);
        command.SetAction(async (parseResult, ct) =>
        {
            var penn = services.GetRequiredService<PenningtonOptions>();
            var localization = penn.Localization;
            var bookOptions = services.GetRequiredService<BookOptions>();
            var navigationBuilder = services.GetRequiredService<NavigationBuilder>();
            var toc = await services.GetServices<IContentService>().CollectTocEntriesAsync();

            var effectiveLocale = parseResult.GetValue(localeOption)
                ?? (localization.IsMultiLocale ? localization.DefaultLocale : null);

            var books = bookOptions.ResolveBooks(penn);
            var siteTitle = string.IsNullOrWhiteSpace(penn.SiteTitle) ? "(untitled)" : penn.SiteTitle;
            await output.WriteLineWithCancellationAsync($"{siteTitle}  ({books.Count} book{(books.Count == 1 ? "" : "s")})", ct: ct);
            await output.WriteLineWithCancellationAsync(ct: ct);

            foreach (var book in books)
            {
                var scoped = BookScoping.ScopeToc(toc, book.NormalizedRoutePrefix, localization, effectiveLocale);
                var tree = await navigationBuilder.BuildTreeAsync(scoped, currentPath: null, locale: effectiveLocale);
                var pdfPath = BookRoutes.PdfPath(book.EffectiveSlug, effectiveLocale, localization.DefaultLocale);

                await output.WriteLineWithCancellationAsync(book.Title, ct: ct);
                await output.WriteLineWithCancellationAsync($"  slug:   {book.EffectiveSlug}", ct: ct);
                await output.WriteLineWithCancellationAsync($"  prefix: {book.NormalizedRoutePrefix}", ct: ct);
                await output.WriteLineWithCancellationAsync($"  output: {pdfPath}", ct: ct);
                await output.WriteLineWithCancellationAsync($"  pages:  {CountPages(tree)}", ct: ct);

                // Mirror the composer's unwrap so the printed tree matches the book's chapters.
                // A routeless wrapper has no page and never reaches the composed book.
                var (intro, chapters) = BookScoping.UnwrapBookRoot(tree, book);
                var display = intro is null || string.IsNullOrEmpty(intro.Route.CanonicalPath.Value)
                        ? chapters
                        : [intro, .. chapters];
                if (display.Count > 0)
                {
                    AsciiTreeWriter.Write(output, display, Label, node => node.Children);
                }

                await output.WriteLineWithCancellationAsync(ct);
            }

            return 0;
        });

        return command;
    }

    private static string Label(NavigationTreeItem node)
    {
        var path = node.Route.CanonicalPath.Value;
        return string.IsNullOrEmpty(path) ? node.Title : $"{node.Title}  {path}";
    }

    private static int CountPages(ImmutableList<NavigationTreeItem> tree)
    {
        var count = 0;
        Walk(tree);
        return count;

        void Walk(ImmutableList<NavigationTreeItem> nodes)
        {
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.Route.CanonicalPath.Value))
                {
                    count++;
                }

                Walk(node.Children);
            }
        }
    }
}

/// <summary>
/// TextWriter extensions for .NET 11.
/// </summary>
/// <remarks>
/// .NET 11 wants the cancellation token to be passed in as a separate parameter, but .NET 10
///  doesn't have the overload. Quick helper until we can drop .NET 11.
/// </remarks>
static class TextWriterExtensions
{
    public static async Task WriteLineWithCancellationAsync(this TextWriter writer, string value, CancellationToken ct = default)
    {
#if NET11_0_OR_GREATER
        await writer.WriteLineAsync(value, ct);
#else
        await writer.WriteLineAsync(value.AsMemory(), ct);
#endif
    }

    public static async Task WriteLineWithCancellationAsync(this TextWriter writer, CancellationToken ct = default)
    {
#if NET11_0_OR_GREATER
        await writer.WriteLineAsync(ct);
#else
        await writer.WriteLineAsync();
#endif
    }
}
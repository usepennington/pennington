namespace Pennington.Book;

using System.Collections.Immutable;
using System.CommandLine;
using Cli;
using Content;
using Infrastructure;
using Localization;
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
        command.SetAction(async (parseResult, _) =>
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
            await output.WriteLineAsync($"{siteTitle}  ({books.Count} book{(books.Count == 1 ? "" : "s")})");
            await output.WriteLineAsync();

            foreach (var book in books)
            {
                var scoped = BookScoping.ScopeToc(toc, book.NormalizedRoutePrefix, localization, effectiveLocale);
                var tree = await navigationBuilder.BuildTreeAsync(scoped, currentPath: null, locale: effectiveLocale);
                var pdfPath = BookRoutes.PdfPath(book.EffectiveSlug, effectiveLocale, localization.DefaultLocale);

                await output.WriteLineAsync(book.Title);
                await output.WriteLineAsync($"  slug:   {book.EffectiveSlug}");
                await output.WriteLineAsync($"  prefix: {book.NormalizedRoutePrefix}");
                await output.WriteLineAsync($"  output: {pdfPath}");
                await output.WriteLineAsync($"  pages:  {CountPages(tree)}");

                // Mirror the composer's unwrap so the printed tree matches the book's chapters.
                // A routeless wrapper has no page and never reaches the composed book.
                var (intro, chapters) = BookScoping.UnwrapBookRoot(tree, book);
                ImmutableList<NavigationTreeItem> display =
                    intro is null || string.IsNullOrEmpty(intro.Route.CanonicalPath.Value)
                        ? chapters
                        : [intro, .. chapters];
                if (display.Count > 0)
                {
                    AsciiTreeWriter.Write(output, display, Label, node => node.Children);
                }

                await output.WriteLineAsync();
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

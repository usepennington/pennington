namespace Pennington.UI.Styling;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pennington.Cli;

/// <summary>DI registration for the <see cref="StyleRegistry"/> and its <c>diag styles</c> command.</summary>
public static class StyleServiceExtensions
{
    /// <summary>
    /// Registers the <see cref="StyleRegistry"/>. Site templates call this internally with their
    /// skin; bare hosts composing Pennington.UI components directly can call it themselves (the
    /// components fall back to the built-in defaults when nothing is registered). When called
    /// more than once the last registration wins; the diag command is registered once.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="templateSkin">Per-slot replacements a site template applies over the component defaults.</param>
    /// <param name="styleOverrides">Factory returning the consumer's per-slot overrides, re-invoked per resolve.</param>
    /// <param name="classMerger">
    /// Tailwind-aware class merge for the override layer — typically
    /// <c>MonorailCssService.CreateClassMerger(...)</c>. When null an override is appended without
    /// conflict resolution; site templates always supply one so overrides knock out conflicting
    /// skin/default utilities.
    /// </param>
    public static IServiceCollection AddPenningtonStyles(
        this IServiceCollection services,
        IReadOnlyDictionary<string, string>? templateSkin = null,
        Func<IReadOnlyDictionary<string, string>?>? styleOverrides = null,
        Func<string, string, string>? classMerger = null)
    {
        // Fail fast: an unknown override key throws here, at startup, not on first render.
        _ = StyleRegistry.Create(templateSkin, styleOverrides?.Invoke(), classMerger);

        // Transient so the overrides factory re-runs per resolve — Styles edits in the
        // consumer's options factory flow through under dotnet run, mirroring AddMonorailCss.
        services.AddTransient(_ => StyleRegistry.Create(templateSkin, styleOverrides?.Invoke(), classMerger));

        // TryAddEnumerable dedups when both a template and the host call this — two diag
        // subcommands named "styles" would collide when the CLI builds the diag group.
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDiagCommand, DiagStylesCommand>());

        return services;
    }
}

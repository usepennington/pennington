namespace Pennington.UI.Styling;

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Cli;

/// <summary><c>diag styles</c> — every style-registry slot, the layer that supplied it, and its effective classes.</summary>
internal sealed class DiagStylesCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "styles";

    /// <inheritdoc/>
    public string Description => "List the style-registry slots with their source layer and effective CSS classes.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var command = new Command(Name, Description);
        command.SetAction((_, _) =>
        {
            StyleRegistry registry;
            try
            {
                registry = services.GetRequiredService<StyleRegistry>();
            }
            catch (InvalidOperationException ex)
            {
                // An unknown override key in the host's options factory throws on resolve.
                output.WriteLine(ex.Message);
                return Task.FromResult(1);
            }

            output.WriteLine($"Style registry — {registry.Entries.Count} slots");
            output.WriteLine();

            var keyWidth = registry.Entries.Max(e => e.Key.Length);
            foreach (var entry in registry.Entries)
            {
                var source = entry.Source switch
                {
                    StyleSource.TemplateSkin => "skin",
                    StyleSource.ConsumerOverride => "override",
                    _ => "default",
                };
                output.WriteLine($"{entry.Key.PadRight(keyWidth)}  {source,-8}  {Display(entry.Effective)}");
                if (entry.Source == StyleSource.ConsumerOverride)
                {
                    output.WriteLine($"{new string(' ', keyWidth)}  base:     {Display(entry.SkinValue ?? entry.DefaultValue)}");
                    output.WriteLine($"{new string(' ', keyWidth)}  override: {Display(entry.OverrideValue!)}");
                }
            }

            return Task.FromResult(0);
        });
        return command;
    }

    private static string Display(string classes) => classes.Length == 0 ? "(empty)" : classes;
}

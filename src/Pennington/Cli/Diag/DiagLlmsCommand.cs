namespace Pennington.Cli.Diag;

using System.CommandLine;
using Infrastructure;
using LlmsTxt;
using Microsoft.Extensions.DependencyInjection;

/// <summary><c>diag llms</c> — preview the generated llms.txt index.</summary>
internal sealed class DiagLlmsCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "llms";

    /// <inheritdoc/>
    public string Description => "Preview the generated llms.txt index for the site.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var command = new Command(Name, Description);
        command.SetAction(async (_, _) =>
        {
            if (services.GetRequiredService<PenningtonOptions>().LlmsTxt is null)
            {
                output.WriteLine("llms.txt is not enabled for this site (call options.AddLlmsTxt(...) to enable it).");
                return 0;
            }

            var content = await services.GetRequiredService<LlmsTxtService>().GetLlmsTxtAsync();
            output.WriteLine(content);
            return 0;
        });
        return command;
    }
}

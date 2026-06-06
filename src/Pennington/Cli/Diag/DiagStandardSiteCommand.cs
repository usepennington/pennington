namespace Pennington.Cli.Diag;

using System.CommandLine;
using Content;
using FrontMatter;
using Microsoft.Extensions.DependencyInjection;
using StandardSite;

/// <summary><c>diag standard-site</c> — validates Standard Site config and per-page document rkeys.</summary>
internal sealed class DiagStandardSiteCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "standard-site";

    /// <inheritdoc/>
    public string Description => "Validate Standard Site (AT Protocol) config and per-page document rkeys.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var command = new Command(Name, Description);
        command.SetAction(async (_, _) =>
        {
            var options = services.GetService<StandardSiteOptions>();
            if (options is null)
            {
                output.WriteLine("Standard Site is not configured (PenningtonOptions.StandardSite is null).");
                return 0;
            }

            var problems = 0;

            if (string.IsNullOrEmpty(options.Did))
            {
                output.WriteLine("ERROR: Did is empty — no verification output will be emitted.");
                problems++;
            }
            else if (!options.Did.StartsWith("did:", StringComparison.Ordinal))
            {
                output.WriteLine($"WARNING: Did '{options.Did}' does not look like a DID (expected 'did:plc:...' or 'did:web:...').");
                problems++;
            }

            if (string.IsNullOrEmpty(options.PublicationRkey))
            {
                output.WriteLine("ERROR: PublicationRkey is empty — no verification output will be emitted.");
                problems++;
            }

            if (!options.IsConfigured)
            {
                output.WriteLine();
                output.WriteLine("Standard Site is present but incompletely configured; emitting nothing (fail-safe).");
                return 1;
            }

            output.WriteLine($"Publication : {AtUri.Build(options.Did, "site.standard.publication", options.PublicationRkey)}");
            output.WriteLine($"Well-known  : /.well-known/site.standard.publication{options.PublicationPath.TrimEnd('/')}");
            if (options.EmitAtprotoDid)
            {
                output.WriteLine($"Handle      : /.well-known/atproto-did -> {options.Did}");
            }

            output.WriteLine();

            var registry = services.GetService<ContentRecordRegistry>();
            if (registry is null)
            {
                output.WriteLine("(No content registry available; skipping per-page rkey check.)");
                return problems == 0 ? 0 : 1;
            }

            var snapshot = await registry.GetSnapshotAsync();
            var capablePages = 0;
            var linkedPages = 0;
            foreach (var (path, record) in snapshot)
            {
                if (record.Metadata is not IStandardSiteDocument)
                {
                    continue;
                }

                capablePages++;
                if (string.IsNullOrEmpty(options.DocumentRkeyResolver(record)))
                {
                    output.WriteLine($"  (no rkey) /{path}");
                }
                else
                {
                    linkedPages++;
                }
            }

            output.WriteLine();
            output.WriteLine($"{linkedPages}/{capablePages} document-capable page(s) link to a site.standard.document record.");
            return problems == 0 ? 0 : 1;
        });

        return command;
    }
}

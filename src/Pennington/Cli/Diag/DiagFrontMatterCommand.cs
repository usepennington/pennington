namespace Pennington.Cli.Diag;

using System.CommandLine;
using System.Reflection;
using Content;
using FrontMatter;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;

/// <summary><c>diag frontmatter</c> — inventory of front-matter keys accepted by each content type in use.</summary>
internal sealed class DiagFrontMatterCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "frontmatter";

    /// <inheritdoc/>
    public string Description => "Inventory the front-matter keys each content type accepts, and how many pages use each type.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var command = new Command(Name, Description);
        command.SetAction(async (_, cancellationToken) =>
        {
            var countByType = new Dictionary<Type, int>();
            await foreach (var item in services.GetServices<IContentService>().ParseAllContentAsync(cancellationToken))
            {
                var type = item.Metadata.GetType();
                countByType[type] = countByType.GetValueOrDefault(type) + 1;
            }

            if (countByType.Count == 0)
            {
                output.WriteLine("No parseable markdown content found.");
                return 0;
            }

            var strict = services.GetRequiredService<PenningtonOptions>().FrontMatter.StrictUnknownKeys;
            output.WriteLine($"Front matter — strict unknown keys: {(strict ? "on" : "off")}");
            output.WriteLine();

            foreach (var (type, count) in countByType.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key.Name, StringComparer.Ordinal))
            {
                var capabilities = Capabilities(type);
                var capabilityNote = capabilities.Count > 0 ? $"  [{string.Join(", ", capabilities)}]" : "";
                output.WriteLine($"{type.Name}  ({count} page{(count == 1 ? "" : "s")}){capabilityNote}");

                foreach (var property in type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetIndexParameters().Length == 0)
                    .OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    output.WriteLine($"  {property.Name}: {FriendlyType(property.PropertyType)}");
                }

                output.WriteLine();
            }

            return 0;
        });
        return command;
    }

    private static List<string> Capabilities(Type type)
    {
        var capabilities = new List<string>();
        if (typeof(ITaggable).IsAssignableFrom(type))
        {
            capabilities.Add("tags");
        }

        if (typeof(IOrderable).IsAssignableFrom(type))
        {
            capabilities.Add("order");
        }

        if (typeof(ISectionable).IsAssignableFrom(type))
        {
            capabilities.Add("section");
        }

        if (typeof(IRedirectable).IsAssignableFrom(type))
        {
            capabilities.Add("redirect");
        }

        return capabilities;
    }

    private static string FriendlyType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
        {
            return FriendlyType(underlying) + "?";
        }

        if (type.IsArray)
        {
            return FriendlyType(type.GetElementType()!) + "[]";
        }

        return type == typeof(string) ? "string"
            : type == typeof(bool) ? "bool"
            : type == typeof(int) ? "int"
            : type == typeof(DateTime) ? "DateTime"
            : type.Name;
    }
}

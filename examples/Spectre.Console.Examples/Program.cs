using System.Reflection;
using Spectre.Console;
using Spectre.Console.Examples;

if (args.Length == 0)
{
    AnsiConsole.MarkupLine("[red]Usage: dotnet run <example-name>[/]");
    AnsiConsole.MarkupLine("[yellow]Available examples:[/]");

    var exampleTypes = GetExampleTypes();
    foreach (var type in exampleTypes)
    {
        var name = type.Name.Replace("Example", "").ToKebabCase();
        AnsiConsole.MarkupLine($"  [cyan]{name}[/]");
    }
    return;
}

var exampleName = args[0];
var exampleType = GetExampleTypes()
    .FirstOrDefault(t => t.Name.Replace("Example", "").ToKebabCase() == exampleName);

if (exampleType == null)
{
    AnsiConsole.MarkupLine($"[red]Example '{exampleName}' not found.[/]");
    return;
}

var example = (IExample)Activator.CreateInstance(exampleType)!;
var remainingArgs = args.Skip(1).ToArray();
example.Run(remainingArgs);

static IEnumerable<Type> GetExampleTypes()
{
    return Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => typeof(IExample).IsAssignableFrom(t) && !t.IsInterface)
        .OrderBy(t => t.FullName);
}

public static class StringExtensions
{
    public static string ToKebabCase(this string input)
    {
        return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString()))
            .ToLower();
    }
}
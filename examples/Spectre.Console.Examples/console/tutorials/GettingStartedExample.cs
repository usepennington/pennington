namespace Spectre.Console.Examples.Console.Tutorials;

/// <summary>
/// A beginner-friendly example that demonstrates creating a simple console application using Spectre.Console.
/// This example covers installation setup, colored "Hello World" output, table creation, text styling, and progress bars.
/// </summary>
public class GettingStartedExample : IExample
{
    public void Run(string[] args)
    {
        AnsiConsole.MarkupLine("[bold green]Getting Started: Building a Rich Console App[/]");
        AnsiConsole.WriteLine();

        // Step 1: Hello World with markup
        ShowColoredHelloWorld();
        AnsiConsole.WriteLine();

        // Step 2: Create a table with data
        ShowDataTable();
        AnsiConsole.WriteLine();

        // Step 3: Text styling with colors and styles
        ShowTextStyling();
        AnsiConsole.WriteLine();

        // Step 4: Progress bar simulation
        ShowProgressBar();
    }

    /// <summary>
    /// Demonstrates basic markup syntax for colored output.
    /// Shows how to use Spectre.Console's markup syntax to display colored text.
    /// </summary>
    public void ShowColoredHelloWorld()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 1: Colored Hello World[/]");
        AnsiConsole.MarkupLine("[green]Hello[/] [red]World[/] from [blue]Spectre.Console[/]!");
        AnsiConsole.MarkupLine("[dim]Using markup syntax for easy text styling[/]");
    }

    /// <summary>
    /// Creates and displays a table with sample data.
    /// Demonstrates table creation, column setup, and row data population.
    /// </summary>
    public void ShowDataTable()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 2: Creating a Data Table[/]");

        var table = new Table()
            .AddColumn("[yellow]Name[/]")
            .AddColumn("[cyan]Language[/]")
            .AddColumn("[green]Experience[/]");

        table.AddRow("Alice", "C#", "5 years");
        table.AddRow("Bob", "Python", "3 years");
        table.AddRow("Carol", "JavaScript", "7 years");
        table.AddRow("David", "Go", "2 years");

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Demonstrates various text styling options including colors, emphasis, and decorations.
    /// Shows different ways to style text using both markup and Style objects.
    /// </summary>
    public void ShowTextStyling()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 3: Text Styling with Colors and Styles[/]");

        // Basic colors
        AnsiConsole.MarkupLine("[red]Red text[/] [green]Green text[/] [blue]Blue text[/]");

        // Text decorations
        AnsiConsole.MarkupLine("[bold]Bold[/] [italic]Italic[/] [underline]Underlined[/] [strikethrough]Strikethrough[/]");

        // Combined styling
        AnsiConsole.MarkupLine("[bold red on yellow]Bold red text on yellow background[/]");

        // Using Style objects
        AnsiConsole.Write("Style object example: ", new Style(Color.Purple, Color.Grey15, Decoration.Bold));
        AnsiConsole.WriteLine("Styled with objects!");
    }

    /// <summary>
    /// Simulates a long-running task with a progress bar.
    /// Demonstrates progress tracking, status updates, and task completion feedback.
    /// </summary>
    public void ShowProgressBar()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 4: Progress Bar for Long-Running Tasks[/]");

        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task1 = ctx.AddTask("[green]Processing files[/]");
                var task2 = ctx.AddTask("[blue]Uploading data[/]");
                var task3 = ctx.AddTask("[yellow]Finalizing[/]");

                while (!ctx.IsFinished)
                {
                    // Simulate work by incrementing progress
                    task1.Increment(2);
                    task2.Increment(1.5);
                    task3.Increment(0.5);

                    Thread.Sleep(50);
                }
            });

        AnsiConsole.MarkupLine("[green]✓[/] All tasks completed successfully!");
    }
}
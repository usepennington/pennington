namespace Spectre.Console.Examples.Console.Tutorials;

/// <summary>
/// An intermediate tutorial focused on building an interactive console dashboard with prompts and live updates.
/// This guides the reader through creating a menu-driven application that asks the user for input and displays dynamic output.
/// Demonstrates Prompt methods, multi-selection lists, Status spinners, and Live Display updates with real-time data.
/// </summary>
public class InteractivePromptAndDashboardTutorialExample : IExample
{
    public void Run(string[] args)
    {
        AnsiConsole.MarkupLine("[bold green]Interactive Prompt and Dashboard Tutorial[/]");
        AnsiConsole.WriteLine();

        var demoMode = args.Contains("--demo") || args.Contains("-d");

        if (demoMode)
        {
            ShowDemoMode();
        }
        else
        {
            ShowInteractiveMode();
        }
    }

    private void ShowDemoMode()
    {
        AnsiConsole.MarkupLine("[yellow]Running in demo mode (non-interactive)[/]");
        AnsiConsole.WriteLine();

        // Step 3: Status spinner during tasks
        ShowStatusSpinner();
        AnsiConsole.WriteLine();

        // Step 4: Live display with dynamic updates
        ShowLiveDisplay();
        AnsiConsole.WriteLine();

        ShowDemoPrompts();
        ShowDemoMultiSelection();
        ShowDemoDashboard();
    }

    private void ShowInteractiveMode()
    {
        // Step 1: Basic prompts and user input
        ShowBasicPrompts();
        AnsiConsole.WriteLine();

        // Step 2: Multi-selection menu
        ShowMultiSelectionMenu();
        AnsiConsole.WriteLine();

        // Step 3: Status spinner during tasks
        ShowStatusSpinner();
        AnsiConsole.WriteLine();

        // Step 4: Live display with dynamic updates
        ShowLiveDisplay();
        AnsiConsole.WriteLine();

        // Step 5: Complete dashboard combining all features
        ShowCompleteDashboard();
    }

    /// <summary>
    /// Demonstrates basic prompt functionality including text input and confirmation dialogs.
    /// Shows how to use AnsiConsole.Ask and AnsiConsole.Confirm for gathering user input.
    /// </summary>
    public void ShowBasicPrompts()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 1: Basic Prompts and User Input[/]");

        // Text input prompt
        var name = AnsiConsole.Ask<string>("What's your [green]name[/]?");
        AnsiConsole.MarkupLine($"Hello, [cyan]{name}[/]!");

        // Numeric input with validation
        var age = AnsiConsole.Ask<int>("How [blue]old[/] are you?");
        AnsiConsole.MarkupLine($"You are [yellow]{age}[/] years old.");

        // Confirmation prompt
        var confirmed = AnsiConsole.Confirm("Do you want to continue?");
        if (confirmed)
        {
            AnsiConsole.MarkupLine("[green]✓[/] Let's continue!");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]✗[/] Maybe next time.");
        }
    }

    /// <summary>
    /// Shows how to create multi-selection menus for choosing from multiple options.
    /// Demonstrates the MultiSelectionPrompt for gathering multiple user choices.
    /// </summary>
    public void ShowMultiSelectionMenu()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 2: Multi-Selection Menu[/]");

        var selectedFeatures = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Which [green]features[/] would you like to enable?")
                .NotRequired() // Allow no selection
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more features)[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle a feature, " +
                                 "[green]<enter>[/] to accept)[/]")
                .AddChoices(new[] {
                    "Dashboard Widgets",
                    "Real-time Updates",
                    "Data Export",
                    "User Analytics",
                    "Custom Themes",
                    "API Integration",
                    "Notification System"
                }));

        if (selectedFeatures.Any())
        {
            AnsiConsole.MarkupLine("[green]Selected features:[/]");
            foreach (var feature in selectedFeatures)
            {
                AnsiConsole.MarkupLine($"  [cyan]•[/] {feature}");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]No features selected.[/]");
        }
    }

    /// <summary>
    /// Demonstrates using Status spinner for long-running operations.
    /// Shows how to update status messages while performing background tasks.
    /// </summary>
    public void ShowStatusSpinner()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 3: Status Spinner During Tasks[/]");

        AnsiConsole.Status()
            .Start("Initializing dashboard...", ctx =>
            {
                Thread.Sleep(1000);

                ctx.Status = "Loading user data...";
                ctx.Spinner(Spinner.Known.Star);
                ctx.SpinnerStyle(Style.Parse("green"));
                Thread.Sleep(1500);

                ctx.Status = "Connecting to services...";
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("blue"));
                Thread.Sleep(1200);

                ctx.Status = "Finalizing setup...";
                ctx.Spinner(Spinner.Known.Clock);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                Thread.Sleep(800);
            });

        AnsiConsole.MarkupLine("[green]✓[/] Dashboard initialization complete!");
    }

    /// <summary>
    /// Shows live display functionality with real-time table updates.
    /// Demonstrates how to create and update a live-rendered table with changing data.
    /// </summary>
    public void ShowLiveDisplay()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 4: Live Display with Dynamic Updates[/]");

        var random = new Random();

        AnsiConsole.Live(CreateStatusTable(0, 0, 0))
            .Start(ctx =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var users = random.Next(50, 200);
                    var requests = random.Next(100, 500);
                    var errors = random.Next(0, 10);

                    ctx.UpdateTarget(CreateStatusTable(users, requests, errors));
                    Thread.Sleep(200);
                }
            });

        AnsiConsole.MarkupLine("[green]✓[/] Live display demo completed!");
    }

    /// <summary>
    /// Combines all interactive features into a complete dashboard experience.
    /// Demonstrates how to build a comprehensive menu-driven application with live updates.
    /// </summary>
    public void ShowCompleteDashboard()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 5: Complete Interactive Dashboard[/]");

        var keepRunning = true;

        while (keepRunning)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Dashboard Menu[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "View System Status",
                        "Check User Activity",
                        "Generate Report",
                        "System Settings",
                        "Exit Dashboard"
                    }));

            switch (choice)
            {
                case "View System Status":
                    ShowSystemStatus();
                    break;

                case "Check User Activity":
                    ShowUserActivity();
                    break;

                case "Generate Report":
                    GenerateReport();
                    break;

                case "System Settings":
                    ShowSettings();
                    break;

                case "Exit Dashboard":
                    if (AnsiConsole.Confirm("Are you sure you want to exit?"))
                    {
                        keepRunning = false;
                        AnsiConsole.MarkupLine("[green]Goodbye![/]");
                    }
                    break;
            }

            if (keepRunning)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Press any key to return to menu...[/]");
                System.Console.ReadKey(true);
                AnsiConsole.Clear();
            }
        }
    }

    private static Table CreateStatusTable(int users, int requests, int errors)
    {
        var table = new Table()
            .AddColumn("[yellow]Metric[/]")
            .AddColumn("[cyan]Current Value[/]")
            .AddColumn("[green]Status[/]");

        table.AddRow("Active Users", users.ToString(), users > 100 ? "[green]Good[/]" : "[yellow]Low[/]");
        table.AddRow("Requests/Min", requests.ToString(), requests > 200 ? "[green]High[/]" : "[blue]Normal[/]");
        table.AddRow("Error Count", errors.ToString(), errors < 5 ? "[green]Good[/]" : "[red]Alert[/]");
        table.AddRow("Uptime", $"{DateTime.Now.Subtract(DateTime.Now.Date):hh\\:mm\\:ss}", "[green]Online[/]");

        return table;
    }

    private static void ShowSystemStatus()
    {
        AnsiConsole.MarkupLine("[bold blue]System Status Overview[/]");

        var statusTable = new Table()
            .AddColumn("[yellow]Component[/]")
            .AddColumn("[cyan]Status[/]")
            .AddColumn("[green]Last Updated[/]");

        statusTable.AddRow("Web Server", "[green]Online[/]", DateTime.Now.AddMinutes(-2).ToString("HH:mm:ss"));
        statusTable.AddRow("Database", "[green]Connected[/]", DateTime.Now.AddMinutes(-1).ToString("HH:mm:ss"));
        statusTable.AddRow("Cache", "[yellow]Warming[/]", DateTime.Now.ToString("HH:mm:ss"));
        statusTable.AddRow("Queue", "[green]Processing[/]", DateTime.Now.AddSeconds(-30).ToString("HH:mm:ss"));

        AnsiConsole.Write(statusTable);
    }

    private static void ShowUserActivity()
    {
        AnsiConsole.MarkupLine("[bold blue]User Activity Monitor[/]");

        var activityChart = new BarChart()
            .Width(60)
            .Label("[green bold underline]Hourly Activity[/]")
            .CenterLabel();

        var random = new Random();
        for (int hour = 0; hour < 24; hour++)
        {
            var activity = random.Next(10, 100);
            activityChart.AddItem($"{hour:00}h", activity, Color.FromInt32(random.Next(1, 255)));
        }

        AnsiConsole.Write(activityChart);
    }

    private static void GenerateReport()
    {
        AnsiConsole.MarkupLine("[bold blue]Generating System Report[/]");

        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var tasks = new[]
                {
                    ctx.AddTask("[green]Collecting user data[/]"),
                    ctx.AddTask("[blue]Analyzing performance[/]"),
                    ctx.AddTask("[yellow]Generating charts[/]"),
                    ctx.AddTask("[purple]Compiling report[/]")
                };

                var random = new Random();
                while (!ctx.IsFinished)
                {
                    foreach (var task in tasks)
                    {
                        if (!task.IsFinished)
                        {
                            task.Increment(random.NextDouble() * 10);
                        }
                    }
                    Thread.Sleep(100);
                }
            });

        AnsiConsole.MarkupLine("[green]✓[/] Report generated successfully!");
    }

    private static void ShowSettings()
    {
        AnsiConsole.MarkupLine("[bold blue]System Settings[/]");

        var theme = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose dashboard [green]theme[/]:")
                .AddChoices("Dark Mode", "Light Mode", "High Contrast", "Colorful"));

        var autoRefresh = AnsiConsole.Confirm("Enable auto-refresh?");
        var refreshInterval = 30;

        if (autoRefresh)
        {
            refreshInterval = AnsiConsole.Ask("Refresh interval (seconds):", 30);
        }

        AnsiConsole.MarkupLine($"[green]✓[/] Theme set to: [cyan]{theme}[/]");
        AnsiConsole.MarkupLine($"[green]✓[/] Auto-refresh: [cyan]{(autoRefresh ? $"Enabled ({refreshInterval}s)" : "Disabled")}[/]");
    }

    private static void ShowDemoPrompts()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 1: Basic Prompts Demo[/]");
        AnsiConsole.MarkupLine("Simulating user input:");
        AnsiConsole.MarkupLine("  What's your [green]name[/]? → [cyan]Alice[/]");
        AnsiConsole.MarkupLine("  How [blue]old[/] are you? → [yellow]28[/]");
        AnsiConsole.MarkupLine("  Do you want to continue? → [green]Yes[/]");
        AnsiConsole.WriteLine();
    }

    private static void ShowDemoMultiSelection()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 2: Multi-Selection Demo[/]");
        AnsiConsole.MarkupLine("Simulating feature selection:");
        AnsiConsole.MarkupLine("[green]Selected features:[/]");
        AnsiConsole.MarkupLine("  [cyan]•[/] Dashboard Widgets");
        AnsiConsole.MarkupLine("  [cyan]•[/] Real-time Updates");
        AnsiConsole.MarkupLine("  [cyan]•[/] API Integration");
        AnsiConsole.WriteLine();
    }

    private static void ShowDemoDashboard()
    {
        AnsiConsole.MarkupLine("[bold yellow]Step 5: Dashboard Demo[/]");
        AnsiConsole.MarkupLine("Simulating dashboard navigation:");
        AnsiConsole.MarkupLine("  1. View System Status → [green]Showing system components[/]");
        AnsiConsole.MarkupLine("  2. Check User Activity → [blue]Displaying activity charts[/]");
        AnsiConsole.MarkupLine("  3. Generate Report → [yellow]Creating performance report[/]");
        AnsiConsole.MarkupLine("[green]✓[/] Dashboard demo completed!");
    }
}
---
title: "Interactive Prompt and Dashboard Tutorial"
description: "An intermediate tutorial focused on building an interactive console dashboard with prompts and live updates"
date: 2025-08-05
tags: ["tutorial", "interactive", "dashboard", "prompts", "live-display"]
section: "Console"
uid: "console-interactive-dashboard"
order: 1040
---

Welcome to building your first interactive console dashboard! This tutorial will guide you through creating a dynamic, menu-driven application that feels responsive and professional. By the end, you'll have built a complete dashboard that accepts user input, shows live updates, and provides an intuitive navigation experience.

## What You'll Build

You're going to create an interactive dashboard that demonstrates the power of Spectre.Console's user interaction features:

- **Smart prompts** that collect user input with validation
- **Multi-selection menus** for choosing multiple options at once  
- **Status spinners** that keep users informed during long operations
- **Live displays** that update in real-time with changing data
- **Complete dashboard** with menu navigation and multiple screens

This isn't just about displaying information—you'll learn to create applications that truly engage with your users!

## Before We Start

This is an intermediate tutorial, so you should be comfortable with:
- Basic C# programming concepts
- Creating and running console applications
- Understanding of Spectre.Console markup syntax (if not, check out the [Getting Started tutorial](getting-started-building-rich-console-app.md) first!)

Ready? Let's dive in and start building something amazing!

## Step 1: Gathering User Input with Prompts

The foundation of any interactive application is collecting user input reliably. Let's start with the basics—asking for information and handling responses gracefully.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowBasicPrompts
```

This approach gives you three essential patterns:

**Text Input**: `AnsiConsole.Ask<string>()` handles text collection with built-in validation. The generic type parameter means you can ask for numbers, dates, or any type that can be parsed from a string.

**Confirmation Dialogs**: `AnsiConsole.Confirm()` creates yes/no prompts that users can navigate with arrow keys. Perfect for "Are you sure?" moments.

**Type Validation**: Notice how asking for an `int` automatically validates the input—users can't proceed until they enter a valid number. No more manual parsing or error handling!

The beauty here is that Spectre.Console handles all the edge cases for you. Invalid input, empty responses, and type conversion errors are all managed automatically.

## Step 2: Powerful Multi-Selection Menus

Now let's level up with multi-selection prompts. These are perfect when users need to choose multiple options from a list—think feature toggles, permission settings, or configuration options.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowMultiSelectionMenu
```

Here's what makes this approach so powerful:

**Flexible Selection**: Users can select zero, one, or many options using the spacebar. The `.NotRequired()` method means they can even choose nothing if that makes sense for your application.

**Great User Experience**: The prompt includes helpful instructions and navigation hints. Users immediately understand how to interact with your application.

**Easy Result Processing**: The result is a simple list of strings you can iterate over. No complex parsing or data manipulation needed.

**Customizable Display**: You can control page size, instruction text, and visual styling to match your application's personality.

This pattern works brilliantly for any scenario where users need to pick from multiple options!

## Step 3: Status Indicators That Actually Help

Nothing frustrates users more than staring at a blank screen wondering if something's happening. Status spinners solve this beautifully by keeping users informed and engaged.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowStatusSpinner
```

This pattern showcases several important concepts:

**Progressive Updates**: The status message changes as work progresses, giving users a sense of advancement through the process.

**Visual Variety**: Different spinner styles (`Star`, `Dots`, `Clock`) and colors prevent monotony and can indicate different types of work.

**Clear Completion**: The success message confirms that the operation finished successfully, providing closure to the user experience.

**Non-Blocking Operations**: While we're using `Thread.Sleep()` for demonstration, this pattern works perfectly with async operations, network calls, or file processing.

Status indicators transform potentially frustrating wait times into reassuring progress updates. Your users will appreciate knowing that things are working as expected!

## Step 4: Live Data with Real-Time Updates

Here's where things get exciting—live displays that update while users watch! This creates dashboard-like experiences that feel dynamic and professional.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowLiveDisplay
```

The magic happens through several key techniques:

**Live Rendering**: `AnsiConsole.Live()` creates a region that can be updated without clearing the entire screen. This creates smooth, flicker-free updates.

**Dynamic Content**: The `CreateStatusTable()` helper method generates fresh table content with each update. You can update any renderable—tables, charts, panels, or custom widgets.

**Real-Time Feel**: Updates happen frequently enough (every 200ms) to feel live, but not so fast that they're distracting or hard to read.

**Meaningful Data**: The example shows system metrics that change realistically—user counts, request rates, error counts. Users can see patterns and trends emerge.

This approach is perfect for monitoring dashboards, progress tracking, or any scenario where data changes over time!

## Step 5: Bringing It All Together

Now for the grand finale—combining everything into a complete interactive dashboard that users can navigate intuitively.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample.ShowCompleteDashboard
```

This complete dashboard demonstrates several important patterns:

**Menu-Driven Navigation**: Users can easily explore different areas of functionality without getting lost. The menu always brings them back to a familiar starting point.

**Contextual Actions**: Each menu option leads to a focused task—viewing status, checking activity, generating reports, or adjusting settings.

**Graceful Exit**: The confirmation prompt before exiting prevents accidental closures and gives users confidence in their actions.

**Clean State Management**: `AnsiConsole.Clear()` and strategic pauses create natural breaks between sections, preventing information overload.

**Consistent Experience**: Every screen follows similar patterns for colors, formatting, and user interaction, creating a cohesive application feel.

## The Complete Experience

Let's see how all these pieces work together in the full application:

```csharp:xmldocid
T:Spectre.Console.Examples.Console.Tutorials.InteractivePromptAndDashboardTutorialExample
```

Perfect! You now have a complete interactive dashboard that demonstrates professional-grade console application patterns. Your users can navigate intuitively, get meaningful feedback, and accomplish their goals efficiently.

## Key Takeaways

You've learned some powerful techniques that will serve you well in any interactive console application:

- **Prompts make input reliable**: Let Spectre.Console handle validation, type conversion, and user experience concerns
- **Multi-selection empowers users**: Give people control over their choices with intuitive selection interfaces  
- **Status indicators build confidence**: Keep users informed during operations so they never wonder what's happening
- **Live displays create engagement**: Real-time updates make applications feel dynamic and responsive
- **Navigation patterns matter**: Consistent menus and clear workflows help users feel confident and productive

## What's Next?

Ready to explore more interactive features? Here are some great next steps:

- **[Showing Progress Bars and Spinners](../how-to/showing-progress-bars-and-spinners.md)**: Learn to track and display progress of long-running tasks
- **[Organizing Layout with Panels and Grids](../how-to/organizing-layout-with-panels-and-grids.md)**: Arrange multiple pieces of output using layout widgets
- **[Prompting for User Input](../how-to/prompting-for-user-input.md)**: Interactively prompt the user for input with validation
- **[Styling Text with Markup and Color](../how-to/styling-text-with-markup-and-color.md)**: Output text with rich styles and colors

You've built something really impressive here—an interactive dashboard that users will actually enjoy using. That's the power of thoughtful user experience design combined with Spectre.Console's robust features!
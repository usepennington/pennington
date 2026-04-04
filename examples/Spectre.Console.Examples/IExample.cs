namespace Spectre.Console.Examples;

/// <summary>
/// Interface for runnable examples that demonstrate Spectre.Console features.
/// </summary>
public interface IExample
{
    /// <summary>
    /// Runs the example with the provided command-line arguments.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the example.</param>
    void Run(string[] args);
}
namespace Pennington.Infrastructure;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

/// <summary>
/// Console formatter used during static-build runs. Emits one line per log
/// entry with no category and no event id — so the output reads like a CLI
/// build tool. Warning and above prefix with <c>warn:</c> / <c>error:</c> /
/// <c>fatal:</c> so they don't blend into the progress stream; Information
/// is plain text.
/// </summary>
internal sealed class BuildConsoleFormatter : ConsoleFormatter
{
    public const string Name = "pennington-build";

    public BuildConsoleFormatter() : base(Name) { }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (string.IsNullOrEmpty(message) && logEntry.Exception is null)
            return;

        var prefix = logEntry.LogLevel switch
        {
            LogLevel.Warning => "warn: ",
            LogLevel.Error => "error: ",
            LogLevel.Critical => "fatal: ",
            _ => "",
        };

        textWriter.Write(prefix);
        textWriter.WriteLine(message);

        if (logEntry.Exception is not null)
        {
            textWriter.WriteLine(logEntry.Exception);
        }
    }
}

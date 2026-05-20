namespace Pennington.Generation;

using Infrastructure;
using Routing;

/// <summary>
/// Options controlling the static build output: target directory, base URL, and cleanup behavior.
/// </summary>
public sealed class OutputOptions
{
    /// <summary>Directory where generated output is written.</summary>
    public required FilePath OutputDirectory { get; init; }

    /// <summary>Base URL the site is deployed under (used to rewrite links in generated HTML).</summary>
    public UrlPath BaseUrl { get; init; } = new("/");

    /// <summary>When true, the output directory is cleared before a build run.</summary>
    public bool CleanOutput { get; init; } = true;

    /// <summary>
    /// Parses CLI arguments into <see cref="OutputOptions"/>, honoring positional and named flag forms.
    /// </summary>
    public static OutputOptions FromArgs(string[] args)
    {
        // OutputOptions is only meaningful for the "build" CLI verb — the same
        // sentinel RunOrBuildAsync uses. In any other invocation (dev run,
        // tests, tooling) the positional args belong to something else (xunit
        // runner, Kestrel flags, …) and must not be interpreted as a base URL
        // or output directory. Previously we blindly read args[1]/args[2],
        // which under `dotnet test` pulled a temp path into BaseUrl and
        // corrupted every rewritten <a href> in integration tests.
        if (!PenningtonBuildMode.IsBuildMode(args))
        {
            return new OutputOptions { OutputDirectory = new FilePath("output") };
        }

        // Supported shapes (all relative to args[0] = "build"):
        //   build                                  -> defaults (/, output)
        //   build /sub                             -> positional baseUrl
        //   build /sub dist                        -> positional baseUrl + output
        //   build --base-url /sub                  -> named flag (space)
        //   build --base-url=/sub                  -> named flag (equals)
        //   build --output dist                    -> named flag
        //   build --output=dist --base-url=/sub    -> mix, any order
        // Named flags win over positional when both appear.
        string? baseUrl = null;
        string? outputDir = null;
        var positional = new List<string>();

        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (TryReadFlag(a, "--base-url", args, ref i, out var baseUrlValue))
            {
                baseUrl = baseUrlValue;
            }
            else if (TryReadFlag(a, "--output", args, ref i, out var outputValue))
            {
                outputDir = outputValue;
            }
            else if (!a.StartsWith('-'))
            {
                positional.Add(a);
            }
            // Unknown flags are ignored — matches the pre-existing silence on
            // stray dev-mode args like --urls=… that xunit / dotnet watch emit.
        }

        // Positional fills slots in declaration order: first fills baseUrl,
        // second fills outputDir. If baseUrl came from a flag, the first
        // positional promotes to outputDir — lets users mix `--base-url=/sub`
        // with a positional output directory without the flag "consuming"
        // a positional slot.
        var slots = new Queue<string>(positional);
        if (baseUrl is null && slots.Count > 0)
        {
            baseUrl = slots.Dequeue();
        }

        if (outputDir is null && slots.Count > 0)
        {
            outputDir = slots.Dequeue();
        }

        return new OutputOptions
        {
            OutputDirectory = new FilePath(outputDir ?? "output"),
            BaseUrl = new UrlPath(NormalizeBaseUrl(baseUrl) ?? "/"),
        };
    }

    /// <summary>
    /// Normalizes a base-URL argument and warns on shapes that suggest the
    /// shell mangled the input. Accepts bare segments (e.g. <c>my-app</c>) and
    /// promotes them to <c>/my-app</c> so POSIX shells can pass the value
    /// without a leading slash that MSYS would otherwise rewrite to a Windows
    /// path. Returns <c>null</c> when <paramref name="raw"/> is null/empty.
    /// </summary>
    internal static string? NormalizeBaseUrl(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        // Windows-absolute path leaked into the base URL — almost certainly
        // Git Bash's MSYS path translation rewriting a leading-slash argument
        // (e.g. `dotnet run -- build /my-app` becomes
        // `C:/Program Files/Git/my-app` before .NET sees it). Warn loudly and
        // strip the prefix so the deploy still produces something usable.
        if (raw.Length >= 3 && char.IsLetter(raw[0]) && raw[1] == ':' && (raw[2] == '/' || raw[2] == '\\'))
        {
            Console.Error.WriteLine(
                $"warning: base-URL '{raw}' looks like a Windows absolute path — your shell likely rewrote a leading '/'. " +
                $"Pass `--base-url <segment>` (no leading slash) or set MSYS_NO_PATHCONV=1.");
            // Take the last segment as a best-effort recovery so links aren't
            // completely broken even when the warning is ignored.
            var lastSep = raw.LastIndexOfAny(['/', '\\']);
            raw = lastSep >= 0 ? raw[(lastSep + 1)..] : raw;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return "/";
            }
        }

        // POSIX-friendly form: `--base-url my-app` is accepted as `/my-app`.
        return raw.StartsWith('/') ? raw : "/" + raw;
    }

    /// <summary>
    /// Reads `--flag=value` or `--flag value` from <paramref name="args"/>.
    /// Returns true and advances <paramref name="i"/> past any consumed second
    /// token. <paramref name="value"/> is null only when <paramref name="token"/>
    /// is an exact match but no subsequent token exists.
    /// </summary>
    private static bool TryReadFlag(string token, string flagName, string[] args, ref int i, out string? value)
    {
        var eqForm = flagName + "=";
        if (token.StartsWith(eqForm, StringComparison.OrdinalIgnoreCase))
        {
            value = token[eqForm.Length..];
            return true;
        }
        if (token.Equals(flagName, StringComparison.OrdinalIgnoreCase))
        {
            if (i + 1 < args.Length)
            {
                value = args[++i];
                return true;
            }
            value = null;
            return true;
        }
        value = null;
        return false;
    }
}
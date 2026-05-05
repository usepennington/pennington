// Throwaway diagnostic. Run AFTER `dotnet run --project docs/Pennington.Docs -- build`.
// Compares the set of class tokens that land in static HTML against the set of class
// selectors emitted in styles.css. Anything in HTML but not in CSS is a likely
// MonorailCss.Discovery gap.
//
//   dotnet run --file tools/css-audit.cs [output-dir]
//
// Default output-dir is docs/Pennington.Docs/output.

using System.Text.RegularExpressions;

var outputDir = args.Length > 0
    ? args[0]
    : Path.Combine("docs", "Pennington.Docs", "output");

if (!Directory.Exists(outputDir))
{
    Console.Error.WriteLine($"output dir not found: {outputDir}");
    Console.Error.WriteLine("did you run `dotnet run --project docs/Pennington.Docs -- build`?");
    return 2;
}

var stylesPath = Path.Combine(outputDir, "styles.css");
if (!File.Exists(stylesPath))
{
    Console.Error.WriteLine($"styles.css not found at {stylesPath}");
    return 2;
}

// classes the runtime/JS adds and that won't appear in the discovery scan, plus
// markers that intentionally have no rule
var ignored = new HashSet<string>(StringComparer.Ordinal)
{
    "humans-only",  // content-extraction marker, no rule by design
    "dark",         // theme state on <html>
    "light",
};

// ---- HTML side -----------------------------------------------------------

var classAttr = new Regex(@"class\s*=\s*""([^""]*)""", RegexOptions.Compiled);
// strip code samples first — pages like the styling tutorial render literal
// `class="mt-12 pt-4 ..."` as text content inside <pre><code>, and the literal
// quotes are NOT html-encoded by the highlighter, so the class attr regex above
// would otherwise match them as if they were real attributes.
var preBlock = new Regex(@"<pre\b[^>]*>.*?</pre>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
var inlineCode = new Regex(@"<code\b[^>]*>.*?</code>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
var htmlClasses = new Dictionary<string, string>(StringComparer.Ordinal); // class -> first file

foreach (var html in Directory.EnumerateFiles(outputDir, "*.html", SearchOption.AllDirectories))
{
    var text = File.ReadAllText(html);
    text = preBlock.Replace(text, "");
    text = inlineCode.Replace(text, "");
    var rel = Path.GetRelativePath(outputDir, html);
    foreach (Match m in classAttr.Matches(text))
    {
        foreach (var cls in m.Groups[1].Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            htmlClasses.TryAdd(cls, rel);
        }
    }
}

// ---- CSS side ------------------------------------------------------------
// .ident — where ident is [\w-] or a backslash-escape \X. Stops at the first
// unescaped non-ident char (`:where(`, `:hover`, `[`, ` `, `,`, `{`, `>`, ...).

var classSelector = new Regex(@"\.((?:[\w-]|\\.)+)", RegexOptions.Compiled);
var cssClasses = new HashSet<string>(StringComparer.Ordinal);

var cssText = File.ReadAllText(stylesPath);
foreach (Match m in classSelector.Matches(cssText))
{
    cssClasses.Add(Unescape(m.Groups[1].Value));
}

static string Unescape(string s)
{
    if (s.IndexOf('\\') < 0) return s;
    var sb = new System.Text.StringBuilder(s.Length);
    for (var i = 0; i < s.Length; i++)
    {
        if (s[i] == '\\' && i + 1 < s.Length) { sb.Append(s[++i]); }
        else { sb.Append(s[i]); }
    }
    return sb.ToString();
}

// ---- Diff ----------------------------------------------------------------

var missing = htmlClasses.Keys
    .Where(c => !cssClasses.Contains(c) && !ignored.Contains(c))
    .OrderBy(c => c, StringComparer.Ordinal)
    .ToList();

var unused = cssClasses
    .Where(c => !htmlClasses.ContainsKey(c) && !ignored.Contains(c))
    .Count();

Console.WriteLine($"output dir : {Path.GetFullPath(outputDir)}");
Console.WriteLine($"HTML classes: {htmlClasses.Count}");
Console.WriteLine($"CSS  classes: {cssClasses.Count}");
Console.WriteLine($"missing rules (HTML − CSS): {missing.Count}");
Console.WriteLine($"unused selectors (CSS − HTML, informational): {unused}");
Console.WriteLine();

if (missing.Count == 0)
{
    Console.WriteLine("OK — every class in HTML has a matching rule in styles.css.");
    return 0;
}

Console.WriteLine("MISSING — classes used in HTML with no rule in styles.css:");
foreach (var c in missing)
{
    Console.WriteLine($"  {c}    (first seen in {htmlClasses[c]})");
}
return 1;

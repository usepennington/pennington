namespace Pennington.Markdown.Extensions;

using Markdig.Syntax;

/// <summary>
/// Extension methods for parsing argument pairs from fenced code block info strings.
/// </summary>
internal static class CodeBlockExtensions
{
    /// <summary>
    /// Parses a string in the format "key=value key2='value with spaces' key3="another value"" into a dictionary.
    /// </summary>
    public static Dictionary<string, string> GetArgumentPairs(this FencedCodeBlock codeBlock)
    {
        if (string.IsNullOrWhiteSpace(codeBlock.Arguments))
        {
            return [];
        }

        var input = codeBlock.Arguments;
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var currentPosition = 0;

        while (currentPosition < input.Length)
        {
            // Skip leading whitespace
            while (currentPosition < input.Length && char.IsWhiteSpace(input[currentPosition]))
                currentPosition++;

            if (currentPosition >= input.Length)
                break;

            // Parse key
            var keyStart = currentPosition;
            while (currentPosition < input.Length && input[currentPosition] != '=')
            {
                if (char.IsWhiteSpace(input[currentPosition]))
                    break;
                currentPosition++;
            }

            if (currentPosition >= input.Length || input[currentPosition] != '=')
                break;

            var key = input.Substring(keyStart, currentPosition - keyStart).Trim();
            currentPosition++; // Skip the equals sign

            // Parse value
            string value;

            // Skip whitespace between equals sign and value
            while (currentPosition < input.Length && char.IsWhiteSpace(input[currentPosition]))
                currentPosition++;

            if (currentPosition >= input.Length)
                break;

            if (input[currentPosition] is '\'' or '"')
            {
                // Quoted value
                var quoteChar = input[currentPosition];
                currentPosition++; // Skip the opening quote
                var valueStart = currentPosition;

                while (currentPosition < input.Length && input[currentPosition] != quoteChar)
                    currentPosition++;

                value = currentPosition >= input.Length
                    ? input.Substring(valueStart)
                    : input.Substring(valueStart, currentPosition - valueStart);

                currentPosition++; // Skip the closing quote if found
            }
            else
            {
                // Unquoted value
                var valueStart = currentPosition;
                while (currentPosition < input.Length && !char.IsWhiteSpace(input[currentPosition]))
                    currentPosition++;

                value = input.Substring(valueStart, currentPosition - valueStart);
            }

            result[key] = value;
        }

        return result;
    }
}

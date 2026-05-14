namespace Pennington.Markdown.Extensions;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

/// <summary>
/// Transforms highlighted HTML code blocks by applying line annotations like
/// [!code highlight], [!code ++], [!code --], [!code focus], [!code error],
/// [!code warning], and [!code word:...] directives.
/// </summary>
internal static class CodeTransformer
{
    // Ordered longest-first so multi-char markers win the LastIndexOf race against
    // their own substrings — e.g. `<!--` must match before `--`, otherwise the `--`
    // inside an HTML comment captures the directive and leaves `<!-- -->` behind.
    private static readonly string[] CommentMarkers = ["<!--", "REM", "//", "--", "/*", "#", "*", "%", "'", ";"];
    private static readonly string[] BlockCommentEndings = ["-->", "*/"];
    private static readonly string[] EmptyCommentPatterns =
        ["<!--", "REM", "//", "--", "/*", "#", "*", "%", "'", ";", "*/", "-->"];

    private record DirectiveMatch(string FullMatch, string Notation, int Index, int EndIndex);
    private record WordHighlightInfo(string Word, string? Message);

    public static string Transform(string highlightedHtml)
    {
        if (string.IsNullOrEmpty(highlightedHtml))
            return highlightedHtml;

        var parser = new HtmlParser();
        var document = parser.ParseDocument(highlightedHtml);

        var preElement = document.QuerySelector("pre");
        if (preElement == null) return highlightedHtml;

        var codeElement = preElement.QuerySelector("code");
        if (codeElement == null) return highlightedHtml;

        var lineElements = StructureCodeIntoLines(codeElement);
        if (lineElements.Count == 0) return highlightedHtml;

        var snippetDirectives = new List<(int LineNumber, DirectiveMatch Directive)>();
        var transformations = new List<LineTransformation>();

        for (var i = 0; i < lineElements.Count; i++)
        {
            var lineElement = lineElements[i];
            var lineText = lineElement.TextContent;
            var directive = FindDirective(lineText);

            if (directive != null)
            {
                if (IsSnippetDirective(directive.Notation))
                {
                    snippetDirectives.Add((i, directive));
                    RemoveDirectiveFromLine(lineElement, directive, lineText);
                }
                else if (directive.Notation.StartsWith("word:", StringComparison.OrdinalIgnoreCase))
                {
                    var wordInfo = ParseWordHighlight(directive.Notation);
                    if (wordInfo != null)
                    {
                        transformations.Add(new LineTransformation { LineNumber = i, Notation = directive.Notation });
                        RemoveDirectiveFromLine(lineElement, directive, lineText);
                        ApplyWordHighlighting(lineElement, wordInfo);
                    }
                    else
                    {
                        RemoveDirectiveFromLine(lineElement, directive, lineText);
                    }
                }
                else
                {
                    transformations.Add(new LineTransformation { LineNumber = i, Notation = directive.Notation });
                    RemoveDirectiveFromLine(lineElement, directive, lineText);
                }
            }
        }

        if (snippetDirectives.Count > 0)
        {
            var validationResult = ValidateAndBuildSnippetRegions(snippetDirectives);
            if (validationResult.IsValid)
            {
                var linesToRemove = DetermineLinesToRemove(lineElements.Count, validationResult.Regions);
                RemoveLinesFromDom(lineElements, linesToRemove);
                transformations = AdjustTransformationsAfterLineRemoval(transformations, linesToRemove);
            }
        }

        ApplyTransformationsToDom(preElement, lineElements, transformations);
        NormalizeLineIndents(lineElements);

        return preElement.OuterHtml;
    }

    private static List<IElement> StructureCodeIntoLines(IElement codeElement)
    {
        var document = codeElement.Owner;
        var lineElements = new List<IElement>();

        var lines = codeElement.InnerHtml.Split('\n');
        codeElement.InnerHtml = "";

        for (var i = 0; i < lines.Length; i++)
        {
            if (i == lines.Length - 1 && string.IsNullOrEmpty(lines[i])) continue;
            if (document == null) continue;

            var lineSpan = document.CreateElement("span");
            lineSpan.ClassName = "line";
            lineSpan.InnerHtml = string.IsNullOrWhiteSpace(lines[i]) ? "  " : lines[i];
            codeElement.AppendChild(lineSpan);
            lineElements.Add(lineSpan);

            if (i < lines.Length - 1)
            {
                codeElement.AppendChild(document.CreateTextNode("\n"));
            }
        }

        return lineElements;
    }

    private static WordHighlightInfo? ParseWordHighlight(string notation)
    {
        if (!notation.StartsWith("word:", StringComparison.OrdinalIgnoreCase))
            return null;

        var content = notation.Substring(5);
        var parts = content.Split('|', 2);

        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
            return null;

        var word = parts[0].Trim();
        var message = parts.Length > 1 ? parts[1].Trim() : null;

        return new WordHighlightInfo(word, string.IsNullOrWhiteSpace(message) ? null : message);
    }

    private static void ApplyWordHighlighting(IElement lineElement, WordHighlightInfo wordInfo)
    {
        var document = lineElement.Owner;
        if (document == null) return;

        var textNodes = lineElement.Descendants().OfType<IText>().ToList();

        foreach (var textNode in textNodes)
        {
            var text = textNode.Text;
            var wordIndex = text.IndexOf(wordInfo.Word, StringComparison.Ordinal);

            if (wordIndex == -1) continue;

            var highlightSpan = document.CreateElement("span");
            highlightSpan.ClassName = wordInfo.Message != null ? "word-highlight-with-message" : "word-highlight";
            highlightSpan.TextContent = wordInfo.Word;

            var beforeText = text.Substring(0, wordIndex);
            var afterText = text.Substring(wordIndex + wordInfo.Word.Length);

            var parent = textNode.Parent;
            if (parent != null)
            {
                if (!string.IsNullOrEmpty(beforeText))
                {
                    parent.InsertBefore(document.CreateTextNode(beforeText), textNode);
                }

                parent.InsertBefore(highlightSpan, textNode);

                if (!string.IsNullOrEmpty(afterText))
                {
                    parent.InsertBefore(document.CreateTextNode(afterText), textNode);
                }

                textNode.Remove();
            }

            if (wordInfo.Message != null)
            {
                AddMessageCallout(lineElement, wordInfo.Message, highlightSpan);
            }

            break;
        }
    }

    private static void AddMessageCallout(IElement lineElement, string message, IElement highlightSpan)
    {
        var document = lineElement.Owner;
        if (document == null) return;

        var messageWrapper = document.CreateElement("span");
        messageWrapper.ClassName = "word-highlight-wrapper";

        var parent = highlightSpan.Parent;
        if (parent != null)
        {
            parent.InsertBefore(messageWrapper, highlightSpan);
            messageWrapper.AppendChild(highlightSpan);
        }

        var messageElement = document.CreateElement("div");
        messageElement.ClassName = "word-highlight-message";
        messageElement.TextContent = message;

        var arrowContainer = document.CreateElement("div");
        arrowContainer.ClassName = "word-highlight-arrow-container";

        var arrowOuter = document.CreateElement("div");
        arrowOuter.ClassName = "word-highlight-arrow-outer";

        var arrowInner = document.CreateElement("div");
        arrowInner.ClassName = "word-highlight-arrow-inner";

        arrowContainer.AppendChild(arrowOuter);
        arrowContainer.AppendChild(arrowInner);
        messageElement.AppendChild(arrowContainer);

        messageWrapper.AppendChild(messageElement);
    }

    private static DirectiveMatch? FindDirective(string text)
    {
        var span = text.AsSpan();
        var codeIndex = span.IndexOf("[!code", StringComparison.OrdinalIgnoreCase);
        if (codeIndex == -1) return null;

        var closeIndex = span[codeIndex..].IndexOf(']');
        if (closeIndex == -1) return null;
        closeIndex += codeIndex;

        var notationStart = codeIndex + 6; // "[!code".Length
        while (notationStart < closeIndex && char.IsWhiteSpace(span[notationStart]))
            notationStart++;

        if (notationStart >= closeIndex) return null;

        var notation = span.Slice(notationStart, closeIndex - notationStart).ToString().Trim();

        var beforeDirective = span[..codeIndex];
        var commentMarkerFound = false;
        var directiveStart = 0;

        foreach (var marker in CommentMarkers)
        {
            var markerIndex = beforeDirective.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex == -1) continue;

            var between = beforeDirective[(markerIndex + marker.Length)..];
            if (IsOnlyWhitespace(between))
            {
                commentMarkerFound = true;
                directiveStart = markerIndex;
                break;
            }
        }

        if (!commentMarkerFound) return null;

        var directiveEnd = closeIndex + 1;
        var afterBracket = span[directiveEnd..];

        var leadingWhitespace = 0;
        while (leadingWhitespace < afterBracket.Length && char.IsWhiteSpace(afterBracket[leadingWhitespace]))
            leadingWhitespace++;

        foreach (var ending in BlockCommentEndings)
        {
            if (afterBracket[leadingWhitespace..].StartsWith(ending, StringComparison.OrdinalIgnoreCase))
            {
                directiveEnd += leadingWhitespace + ending.Length;
                break;
            }
        }

        var fullMatch = span.Slice(directiveStart, directiveEnd - directiveStart).ToString();
        return new DirectiveMatch(fullMatch, notation, directiveStart, directiveEnd);
    }

    private static bool IsOnlyWhitespace(ReadOnlySpan<char> span)
    {
        foreach (var c in span)
        {
            if (!char.IsWhiteSpace(c)) return false;
        }
        return true;
    }

    private static void RemoveDirectiveFromLine(IElement lineElement, DirectiveMatch directive, string fullLineText)
    {
        var shouldPreserve = DetermineCommentPreservation(directive, fullLineText);
        var commentMarker = shouldPreserve ? ExtractCommentMarker(directive) : "";

        if (TryRemoveFromSingleNode(lineElement, directive.FullMatch, commentMarker))
        {
            CleanupLineElement(lineElement);
            return;
        }

        RemoveDirectiveAcrossNodes(lineElement, directive, commentMarker);
        CleanupLineElement(lineElement);
    }

    private static bool DetermineCommentPreservation(DirectiveMatch directive, string fullLineText)
    {
        if (directive.EndIndex >= fullLineText.Length) return false;
        var afterDirective = fullLineText.AsSpan()[directive.EndIndex..];
        return !IsOnlyWhitespace(afterDirective);
    }

    private static string ExtractCommentMarker(DirectiveMatch directive)
    {
        var directiveSpan = directive.FullMatch.AsSpan();
        var codeIndex = directiveSpan.IndexOf("[!code", StringComparison.OrdinalIgnoreCase);
        if (codeIndex == -1) return "";

        var marker = directiveSpan[..codeIndex].ToString().TrimEnd();
        return string.IsNullOrEmpty(marker) ? "" : marker;
    }

    private static bool TryRemoveFromSingleNode(IElement lineElement, string directive, string commentMarker)
    {
        var textNodes = lineElement.Descendants().OfType<IText>().ToList();

        foreach (var node in textNodes)
        {
            if (!node.Text.Contains(directive)) continue;

            var replacement = "";
            if (!string.IsNullOrEmpty(commentMarker))
            {
                var directiveIndex = node.Text.IndexOf(directive, StringComparison.Ordinal);
                var afterDirective = node.Text.Substring(directiveIndex + directive.Length);
                replacement = !string.IsNullOrWhiteSpace(afterDirective) && !afterDirective.StartsWith(' ')
                    ? commentMarker + " "
                    : commentMarker;
            }

            var newText = node.Text.Replace(directive, replacement);

            if (string.IsNullOrEmpty(commentMarker))
            {
                newText = newText.TrimEnd();
                if (IsEmptyComment(newText))
                {
                    newText = "";
                }
            }

            node.TextContent = newText;
            return true;
        }

        return false;
    }

    private static bool IsEmptyComment(string text)
    {
        var trimmed = text.Trim();

        if (EmptyCommentPatterns.Any(pattern => string.Equals(trimmed, pattern, StringComparison.OrdinalIgnoreCase)))
            return true;

        if (trimmed.StartsWith("/*", StringComparison.OrdinalIgnoreCase) &&
            trimmed.EndsWith("*/", StringComparison.OrdinalIgnoreCase))
        {
            var inner = trimmed.Substring(2, trimmed.Length - 4);
            return string.IsNullOrWhiteSpace(inner);
        }

        if (trimmed.StartsWith("<!--", StringComparison.OrdinalIgnoreCase) &&
            trimmed.EndsWith("-->", StringComparison.OrdinalIgnoreCase))
        {
            var inner = trimmed.Substring(4, trimmed.Length - 7);
            return string.IsNullOrWhiteSpace(inner);
        }

        return false;
    }

    private static void RemoveDirectiveAcrossNodes(IElement lineElement, DirectiveMatch directive, string commentMarker)
    {
        var textNodes = lineElement.Descendants().OfType<IText>().ToList();
        var currentPosition = 0;
        var commentMarkerAdded = false;

        foreach (var node in textNodes)
        {
            var nodeStart = currentPosition;
            var nodeEnd = currentPosition + node.Text.Length;

            if (nodeStart < directive.EndIndex && nodeEnd > directive.Index)
            {
                var localStart = Math.Max(0, directive.Index - nodeStart);
                var localEnd = Math.Min(node.Text.Length, directive.EndIndex - nodeStart);

                var before = node.Text[..localStart];
                var after = node.Text[localEnd..];

                if (!string.IsNullOrEmpty(commentMarker) && !commentMarkerAdded)
                {
                    before += commentMarker;
                    commentMarkerAdded = true;
                }

                node.TextContent = (before + after).TrimEnd();
            }

            currentPosition = nodeEnd;
        }
    }

    private static void CleanupLineElement(IElement lineElement)
    {
        var spans = lineElement.QuerySelectorAll("span").ToList();
        var i = 0;

        while (i < spans.Count)
        {
            var span = spans[i];
            var shouldRemove = false;

            if (string.IsNullOrWhiteSpace(span.TextContent) && span.ChildNodes.Length == 0)
            {
                var prev = span.PreviousSibling;
                var next = span.NextSibling;

                shouldRemove = prev == null || next == null ||
                    (prev is IText pt && string.IsNullOrWhiteSpace(pt.TextContent)) ||
                    (next is IText nt && string.IsNullOrWhiteSpace(nt.TextContent));
            }
            else if (IsOrphanedCommentMarker(span))
            {
                shouldRemove = true;
            }

            if (shouldRemove)
            {
                span.Remove();
                spans.RemoveAt(i);
                continue;
            }

            if (i < spans.Count - 1 && TryMergeSpans(span, spans[i + 1]))
            {
                spans.RemoveAt(i + 1);
                continue;
            }

            i++;
        }
    }

    private static bool IsOrphanedCommentMarker(IElement span)
    {
        var content = span.TextContent.Trim();
        if (!IsCommentMarkerOnly(content))
            return false;

        var sibling = span.NextSibling;
        while (sibling != null)
        {
            var hasContent = sibling switch
            {
                IElement elem => !string.IsNullOrWhiteSpace(elem.TextContent),
                IText text => !string.IsNullOrWhiteSpace(text.Text),
                _ => false,
            };

            if (hasContent) return false;
            sibling = sibling.NextSibling;
        }

        return true;
    }

    private static bool IsCommentMarkerOnly(string text) =>
        CommentMarkers.Any(marker => string.Equals(text, marker, StringComparison.OrdinalIgnoreCase));

    private static bool TryMergeSpans(IElement current, IElement next)
    {
        if (current.ClassName != next.ClassName || current.NextElementSibling != next)
            return false;

        var node = current.NextSibling;
        while (node != null && node != next)
        {
            switch (node)
            {
                case IText text when !string.IsNullOrWhiteSpace(text.Text):
                case IElement:
                    return false;
                default:
                    node = node.NextSibling;
                    break;
            }
        }

        var currentContent = current.TextContent;
        var nextContent = next.TextContent;

        if (current.ClassList.Contains("hljs-comment") &&
            IsCommentMarkerOnly(currentContent) &&
            !string.IsNullOrEmpty(nextContent) && !nextContent.StartsWith(' '))
        {
            current.InnerHtml += " " + next.InnerHtml;
        }
        else
        {
            current.InnerHtml += next.InnerHtml;
        }

        next.Remove();
        return true;
    }

    private static void ApplyTransformationsToDom(IElement preElement, List<IElement> lineElements, List<LineTransformation> transformations)
    {
        if (transformations.Count == 0) return;

        var transformationsByType = transformations
            .GroupBy(t => t.Notation)
            .ToDictionary(g => g.Key, g => g.Select(t => t.LineNumber).ToHashSet());

        foreach (var transform in transformations)
        {
            var lineElement = lineElements[transform.LineNumber];
            var cssClass = GetCssClassForNotation(transform.Notation);
            if (cssClass != null)
            {
                lineElement.ClassList.Add(cssClass);
            }
        }

        if (transformationsByType.TryGetValue("focus", out var focusedLineNumbers))
        {
            preElement.ClassList.Add("has-focused");

            for (var i = 0; i < lineElements.Count; i++)
            {
                if (!focusedLineNumbers.Contains(i))
                {
                    lineElements[i].ClassList.Add("blurred");
                }
            }
        }

        if (transformationsByType.ContainsKey("highlight") || transformationsByType.ContainsKey("hl"))
        {
            preElement.ClassList.Add("has-highlighted");
        }

        if (transformationsByType.ContainsKey("++") || transformationsByType.ContainsKey("--"))
        {
            preElement.ClassList.Add("has-diff");
        }

        if (transformationsByType.ContainsKey("error"))
        {
            preElement.ClassList.Add("has-errors");
        }

        if (transformationsByType.ContainsKey("warning"))
        {
            preElement.ClassList.Add("has-warnings");
        }

        foreach (var notation in transformationsByType.Keys)
        {
            if (notation.StartsWith("word:", StringComparison.OrdinalIgnoreCase))
            {
                preElement.ClassList.Add("has-word-highlights");
                break;
            }
        }
    }

    private static void NormalizeLineIndents(List<IElement> lineElements)
    {
        if (lineElements.Count == 0) return;

        var minIndent = int.MaxValue;
        var hasNonEmptyLine = false;

        foreach (var lineElement in lineElements)
        {
            var textContent = lineElement.TextContent;
            if (string.IsNullOrWhiteSpace(textContent)) continue;

            var leadingSpaces = 0;
            while (leadingSpaces < textContent.Length && textContent[leadingSpaces] == ' ')
                leadingSpaces++;

            if (leadingSpaces < minIndent)
                minIndent = leadingSpaces;

            hasNonEmptyLine = true;
        }

        if (!hasNonEmptyLine || minIndent == 0 || minIndent == int.MaxValue) return;

        foreach (var lineElement in lineElements)
        {
            var textContent = lineElement.TextContent;
            if (string.IsNullOrWhiteSpace(textContent)) continue;
            RemoveLeadingSpaces(lineElement, minIndent);
        }
    }

    private static void RemoveLeadingSpaces(IElement lineElement, int count)
    {
        if (lineElement.ChildNodes.Length == 1 && lineElement.FirstChild is IText singleTextNode)
        {
            var text = singleTextNode.Text;
            var spacesToRemove = Math.Min(count, text.Length);
            singleTextNode.TextContent = text.Substring(spacesToRemove);
            return;
        }

        var spacesRemaining = count;

        foreach (var node in lineElement.ChildNodes)
        {
            if (spacesRemaining == 0) break;

            if (node is IText textNode)
            {
                var text = textNode.Text;
                var toRemove = Math.Min(spacesRemaining, text.Length);
                textNode.TextContent = text.Substring(toRemove);
                spacesRemaining -= toRemove;
            }
            else if (node is IElement elementNode)
            {
                var firstText = elementNode.ChildNodes.OfType<IText>().FirstOrDefault();
                if (firstText != null)
                {
                    var text = firstText.Text;
                    var toRemove = Math.Min(spacesRemaining, text.Length);
                    firstText.TextContent = text.Substring(toRemove);
                    spacesRemaining -= toRemove;
                }
            }
        }
    }

    private static string? GetCssClassForNotation(string notation) => notation switch
    {
        "highlight" or "hl" => "highlight",
        "++" => "diff-add",
        "--" => "diff-remove",
        "focus" => "focused",
        "error" => "error",
        "warning" => "warning",
        _ => null,
    };

    private static bool IsSnippetDirective(string notation)
    {
        var lower = notation.ToLowerInvariant();
        return lower is "include-start" or "include-end" or "exclude-start" or "exclude-end";
    }

    private static SnippetValidationResult ValidateAndBuildSnippetRegions(
        List<(int LineNumber, DirectiveMatch Directive)> snippetDirectives)
    {
        var regions = new List<SnippetRegion>();
        var includeStack = new Stack<int>();
        var excludeStack = new Stack<int>();

        foreach (var (lineNumber, directive) in snippetDirectives)
        {
            var notation = directive.Notation.ToLowerInvariant();

            switch (notation)
            {
                case "include-start":
                    if (includeStack.Count > 0)
                        return new SnippetValidationResult([], false);
                    includeStack.Push(lineNumber);
                    break;

                case "include-end":
                    if (includeStack.Count == 0) continue;
                    var includeStart = includeStack.Pop();
                    regions.Add(new SnippetRegion(includeStart, lineNumber, SnippetRegionType.Include));
                    break;

                case "exclude-start":
                    if (excludeStack.Count > 0)
                        return new SnippetValidationResult([], false);
                    excludeStack.Push(lineNumber);
                    break;

                case "exclude-end":
                    if (excludeStack.Count == 0) continue;
                    var excludeStart = excludeStack.Pop();
                    regions.Add(new SnippetRegion(excludeStart, lineNumber, SnippetRegionType.Exclude));
                    break;
            }
        }

        return new SnippetValidationResult(regions, true);
    }

    private static HashSet<int> DetermineLinesToRemove(int totalLineCount, List<SnippetRegion> regions)
    {
        if (regions.Count == 0) return [];

        var includeRegions = regions.Where(r => r.Type == SnippetRegionType.Include).ToList();
        var excludeRegions = regions.Where(r => r.Type == SnippetRegionType.Exclude).ToList();
        var linesToRemove = new HashSet<int>();

        if (includeRegions.Count > 0)
        {
            for (var i = 0; i < totalLineCount; i++)
                linesToRemove.Add(i);

            foreach (var region in includeRegions)
            {
                for (var i = region.StartLine + 1; i < region.EndLine; i++)
                    linesToRemove.Remove(i);
            }
        }

        foreach (var region in excludeRegions)
        {
            for (var i = region.StartLine; i <= region.EndLine; i++)
                linesToRemove.Add(i);
        }

        foreach (var region in includeRegions)
        {
            linesToRemove.Add(region.StartLine);
            linesToRemove.Add(region.EndLine);
        }

        return linesToRemove;
    }

    private static void RemoveLinesFromDom(List<IElement> lineElements, HashSet<int> linesToRemove)
    {
        if (linesToRemove.Count == 0) return;

        for (var i = lineElements.Count - 1; i >= 0; i--)
        {
            if (linesToRemove.Contains(i))
            {
                var lineElement = lineElements[i];
                var nextSibling = lineElement.NextSibling;
                if (nextSibling is IText textNode && textNode.Text == "\n")
                    textNode.Remove();

                lineElement.Remove();
                lineElements.RemoveAt(i);
            }
        }
    }

    private static List<LineTransformation> AdjustTransformationsAfterLineRemoval(
        List<LineTransformation> transformations, HashSet<int> removedLines)
    {
        if (removedLines.Count == 0) return transformations;

        var sortedRemovedLines = removedLines.OrderBy(x => x).ToList();
        var adjusted = new List<LineTransformation>();

        foreach (var transformation in transformations)
        {
            if (removedLines.Contains(transformation.LineNumber)) continue;

            var linesBefore = sortedRemovedLines.Count(r => r < transformation.LineNumber);
            adjusted.Add(new LineTransformation
            {
                LineNumber = transformation.LineNumber - linesBefore,
                Notation = transformation.Notation,
            });
        }

        return adjusted;
    }

    private sealed class LineTransformation
    {
        public int LineNumber { get; init; }
        public string Notation { get; init; } = string.Empty;
    }

    private record SnippetRegion(int StartLine, int EndLine, SnippetRegionType Type);

    private enum SnippetRegionType
    {
        Include,
        Exclude,
    }

    private record SnippetValidationResult(List<SnippetRegion> Regions, bool IsValid);
}
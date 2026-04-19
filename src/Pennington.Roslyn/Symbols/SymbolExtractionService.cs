namespace Pennington.Roslyn.Symbols;

using System.Collections.Concurrent;
using Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Utilities;
using Workspace;

/// <summary>
/// Extracts all symbols from a Roslyn Solution and enables lookup by XML documentation ID.
/// Uses <see cref="AsyncLazy{T}"/> for lazy one-time symbol table loading with retry on failure.
/// </summary>
internal sealed class SymbolExtractionService : ISymbolExtractionService
{
    private readonly ISolutionWorkspaceService _workspaceService;
    private readonly ILogger<SymbolExtractionService> _logger;
    private AsyncLazy<IReadOnlyDictionary<string, SymbolInfo>> _symbolsLazy;

    public SymbolExtractionService(
        ISolutionWorkspaceService workspaceService,
        ILogger<SymbolExtractionService> logger)
    {
        ArgumentNullException.ThrowIfNull(workspaceService);
        ArgumentNullException.ThrowIfNull(logger);

        _workspaceService = workspaceService;
        _logger = logger;
        _symbolsLazy = new AsyncLazy<IReadOnlyDictionary<string, SymbolInfo>>(LoadAllSymbolsAsync);
    }

    public async Task<IReadOnlyDictionary<string, SymbolInfo>> ExtractSymbolsAsync(Solution solution)
    {
        var symbols = new ConcurrentDictionary<string, SymbolInfo>(StringComparer.Ordinal);
        var documentByPath = new ConcurrentDictionary<string, (Document Document, Project Project)>(StringComparer.Ordinal);
        var fallbackByProject = new ConcurrentDictionary<ProjectId, (Document Document, Project Project)>();

        await Parallel.ForEachAsync(solution.Projects, async (project, ct) =>
        {
            var compilation = await project.GetCompilationAsync(ct);
            if (compilation is null)
            {
                _logger.LogWarning("Failed to get compilation for project {ProjectName}", project.Name);
                return;
            }

            await Parallel.ForEachAsync(project.Documents, async (document, ct2) =>
            {
                try
                {
                    if (document.FilePath is { Length: > 0 } path)
                    {
                        documentByPath[path] = (document, project);
                        fallbackByProject.TryAdd(project.Id, (document, project));
                    }
                    await ExtractDocumentSymbolsAsync(document, compilation, project, symbols, ct2);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to extract symbols from {DocumentPath}", document.FilePath);
                }
            });

            // Fallback pass — walk compilation symbols for types the syntax-only
            // pass missed (e.g. C# 15 unions whose declaration node is not a
            // TypeDeclarationSyntax, members synthesized from primary-constructor
            // parameter lists like record `#ctor` overloads, or Razor-generated
            // component classes whose syntax tree path is not a user document).
            try
            {
                await ExtractCompilationSymbolsAsync(compilation, documentByPath, fallbackByProject, project, symbols);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract compilation-level symbols for {ProjectName}", project.Name);
            }
        });

        _logger.LogDebug("Extracted {Count} symbols from solution", symbols.Count);
        return symbols;
    }

    private async Task ExtractCompilationSymbolsAsync(
        Compilation compilation,
        ConcurrentDictionary<string, (Document Document, Project Project)> documentByPath,
        ConcurrentDictionary<ProjectId, (Document Document, Project Project)> fallbackByProject,
        Project currentProject,
        ConcurrentDictionary<string, SymbolInfo> symbols)
    {
        var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var namespaceQueue = new Queue<INamespaceSymbol>();
        var typeQueue = new Queue<INamedTypeSymbol>();
        namespaceQueue.Enqueue(compilation.Assembly.GlobalNamespace);

        while (namespaceQueue.Count > 0)
        {
            var ns = namespaceQueue.Dequeue();
            foreach (var member in ns.GetMembers())
            {
                switch (member)
                {
                    case INamespaceSymbol child:
                        namespaceQueue.Enqueue(child);
                        break;
                    case INamedTypeSymbol type when visited.Add(type):
                        typeQueue.Enqueue(type);
                        break;
                }
            }
        }

        while (typeQueue.Count > 0)
        {
            var type = typeQueue.Dequeue();
            await TryAddSymbolFromCompilationAsync(type, documentByPath, fallbackByProject, currentProject, symbols);
            foreach (var typeMember in type.GetMembers())
            {
                if (typeMember is INamedTypeSymbol nested)
                {
                    if (visited.Add(nested)) typeQueue.Enqueue(nested);
                    continue;
                }
                await TryAddSymbolFromCompilationAsync(typeMember, documentByPath, fallbackByProject, currentProject, symbols);
            }
        }
    }

    private async Task TryAddSymbolFromCompilationAsync(
        ISymbol symbol,
        ConcurrentDictionary<string, (Document Document, Project Project)> documentByPath,
        ConcurrentDictionary<ProjectId, (Document Document, Project Project)> fallbackByProject,
        Project currentProject,
        ConcurrentDictionary<string, SymbolInfo> symbols)
    {
        var docId = symbol.GetDocumentationCommentId();
        if (string.IsNullOrEmpty(docId)) return;

        var normalizedId = XmlDocIdNormalizer.Normalize(docId);
        if (symbols.ContainsKey(normalizedId)) return;

        var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
        var path = syntaxRef?.SyntaxTree.FilePath;
        (Document Document, Project Project) entry;
        SyntaxNode node;
        TextSpan span;

        if (syntaxRef is not null && !string.IsNullOrEmpty(path) && documentByPath.TryGetValue(path, out entry))
        {
            node = await syntaxRef.GetSyntaxAsync();
            span = node.Span;
        }
        else if (syntaxRef is not null && fallbackByProject.TryGetValue(currentProject.Id, out entry))
        {
            // Compiler-synthesized symbols (e.g. record primary constructors whose
            // syntax reference points to the record declaration but whose path
            // lookup misses) register with a placeholder span; downstream source
            // extraction will no-op against it.
            node = await syntaxRef.GetSyntaxAsync();
            span = default;
        }
        else
        {
            return;
        }

        var sourceText = await entry.Document.GetTextAsync();
        var xmlDoc = symbol.GetDocumentationCommentXml();

        var info = new SymbolInfo(
            Symbol: symbol,
            Document: entry.Document,
            SyntaxNode: node,
            SourceText: sourceText,
            TextSpan: span,
            XmlDocumentation: string.IsNullOrWhiteSpace(xmlDoc) ? null : xmlDoc,
            Project: entry.Project
        );

        symbols.TryAdd(normalizedId, info);
    }

    public async Task<SymbolInfo?> FindSymbolAsync(string xmlDocId)
    {
        var normalizedId = XmlDocIdNormalizer.Normalize(xmlDocId);
        var symbols = await _symbolsLazy.Value;

        if (symbols.TryGetValue(normalizedId, out var symbolInfo))
        {
            return symbolInfo;
        }

        _logger.LogTrace("Symbol not found for xmlDocId: {XmlDocId} (normalized: {NormalizedId})", xmlDocId, normalizedId);
        return null;
    }

    public async Task<string> ExtractCodeFragmentAsync(string xmlDocId, bool bodyOnly = false, bool includeLeadingTrivia = true)
    {
        var symbolInfo = await FindSymbolAsync(xmlDocId);
        if (symbolInfo is null)
        {
            _logger.LogWarning("Cannot extract code fragment — symbol not found: {XmlDocId}", xmlDocId);
            return string.Empty;
        }

        var root = await symbolInfo.Document.GetSyntaxRootAsync();
        if (root is null)
        {
            return string.Empty;
        }

        // Find the node in the current syntax tree by its span
        var node = root.FindNode(symbolInfo.TextSpan);
        if (node is null)
        {
            return string.Empty;
        }

        var sourceText = await symbolInfo.Document.GetTextAsync();
        var fullText = sourceText.ToString();

        var fragment = await CodeFragmentExtractor.ExtractCodeFragmentAsync(node, fullText, bodyOnly, includeLeadingTrivia);
        return TextFormatter.NormalizeIndents(fragment);
    }

    public async Task<string> ExtractDeclarationSignatureAsync(string xmlDocId)
    {
        var symbolInfo = await FindSymbolAsync(xmlDocId);
        if (symbolInfo is null)
        {
            _logger.LogWarning("Cannot extract declaration signature — symbol not found: {XmlDocId}", xmlDocId);
            return string.Empty;
        }

        var root = await symbolInfo.Document.GetSyntaxRootAsync();
        if (root is null)
        {
            return string.Empty;
        }

        var node = root.FindNode(symbolInfo.TextSpan);
        if (node is null)
        {
            return string.Empty;
        }

        var sourceText = await symbolInfo.Document.GetTextAsync();
        var fullText = sourceText.ToString();

        var fragment = await CodeFragmentExtractor.ExtractSignatureAsync(node, fullText);
        return TextFormatter.NormalizeIndents(fragment);
    }

    public void ClearCache()
    {
        _logger.LogDebug("Clearing symbol extraction cache");
        _symbolsLazy.Reset();
    }

    private async Task<IReadOnlyDictionary<string, SymbolInfo>> LoadAllSymbolsAsync()
    {
        _logger.LogDebug("Loading all symbols from workspace");

        var projects = await _workspaceService.GetProjectsAsync();
        var solution = projects.FirstOrDefault()?.Solution;

        if (solution is null)
        {
            _logger.LogWarning("No solution available from workspace service");
            return new Dictionary<string, SymbolInfo>();
        }

        return await ExtractSymbolsAsync(solution);
    }

    private async Task ExtractDocumentSymbolsAsync(
        Document document,
        Compilation compilation,
        Project project,
        ConcurrentDictionary<string, SymbolInfo> symbols,
        CancellationToken ct)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(ct);
        if (syntaxRoot is null)
        {
            return;
        }

        var semanticModel = compilation.GetSemanticModel(syntaxRoot.SyntaxTree);
        var sourceText = await document.GetTextAsync(ct);

        // Extract type declarations and their members
        foreach (var typeDecl in syntaxRoot.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            AddSymbol(typeDecl, semanticModel, document, sourceText, project, symbols);

            // Extract members within the type
            foreach (var member in typeDecl.Members)
            {
                AddSymbol(member, semanticModel, document, sourceText, project, symbols);
            }
        }

        // Extract delegate declarations
        foreach (var delegateDecl in syntaxRoot.DescendantNodes().OfType<DelegateDeclarationSyntax>())
        {
            AddSymbol(delegateDecl, semanticModel, document, sourceText, project, symbols);
        }

        // Extract enum declarations and their members
        foreach (var enumDecl in syntaxRoot.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            AddSymbol(enumDecl, semanticModel, document, sourceText, project, symbols);

            foreach (var member in enumDecl.Members)
            {
                AddSymbol(member, semanticModel, document, sourceText, project, symbols);
            }
        }

        // Extract global statements (top-level programs)
        foreach (var globalStatement in syntaxRoot.DescendantNodes().OfType<GlobalStatementSyntax>())
        {
            AddSymbol(globalStatement, semanticModel, document, sourceText, project, symbols);
        }
    }

    private void AddSymbol(
        SyntaxNode node,
        SemanticModel semanticModel,
        Document document,
        SourceText sourceText,
        Project project,
        ConcurrentDictionary<string, SymbolInfo> symbols)
    {
        var symbol = semanticModel.GetDeclaredSymbol(node);
        if (symbol is null)
        {
            return;
        }

        var docId = symbol.GetDocumentationCommentId();
        if (string.IsNullOrEmpty(docId))
        {
            return;
        }

        var normalizedId = XmlDocIdNormalizer.Normalize(docId);

        var xmlDoc = symbol.GetDocumentationCommentXml();

        var info = new SymbolInfo(
            Symbol: symbol,
            Document: document,
            SyntaxNode: node,
            SourceText: sourceText,
            TextSpan: node.Span,
            XmlDocumentation: string.IsNullOrWhiteSpace(xmlDoc) ? null : xmlDoc,
            Project: project
        );

        if (!symbols.TryAdd(normalizedId, info))
        {
            _logger.LogTrace("Duplicate symbol ID: {DocId}", normalizedId);
        }
    }
}
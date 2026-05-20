namespace Pennington.Roslyn.Symbols;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>Background service that kicks off the one-time symbol table walk so the first page request doesn't pay the cold-load cost.</summary>
internal sealed class SymbolExtractionWarmupService : BackgroundService
{
    private readonly ISymbolExtractionService _symbolService;
    private readonly ILogger<SymbolExtractionWarmupService> _logger;

    public SymbolExtractionWarmupService(
        ISymbolExtractionService symbolService,
        ILogger<SymbolExtractionWarmupService> logger)
    {
        _symbolService = symbolService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _symbolService.WarmupAsync(stoppingToken);
            sw.Stop();
            _logger.LogInformation("Symbol extraction warmup completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // App is shutting down before warmup finished — nothing to do.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Symbol extraction warmup failed; first request will retry the walk");
        }
    }
}
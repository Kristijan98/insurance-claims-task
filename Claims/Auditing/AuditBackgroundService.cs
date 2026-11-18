namespace Claims.Auditing;

/// <summary>
/// Reads queued audit entries from <see cref="Auditer"/> and saves them to the audit database in batches.
/// </summary>
public class AuditBackgroundService : BackgroundService
{
    private readonly Auditer _auditer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger;

    /// <summary>
    /// Creates the background service.
    /// </summary>
    public AuditBackgroundService(Auditer auditer, IServiceScopeFactory scopeFactory, ILogger<AuditBackgroundService> logger)
    {
        _auditer = auditer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await _auditer.Reader.WaitToReadAsync(stoppingToken))
            {
                await SaveQueuedEntriesAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Host is stopping; fall through and save whatever is still queued.
        }

        await SaveQueuedEntriesAsync();
    }

    private async Task SaveQueuedEntriesAsync()
    {
        var batch = new List<object>();
        while (_auditer.Reader.TryRead(out var entry))
        {
            batch.Add(entry);
        }

        if (batch.Count == 0)
        {
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
            context.AddRange(batch);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {Count} audit entries.", batch.Count);
        }
    }
}

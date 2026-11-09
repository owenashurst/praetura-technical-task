using Microsoft.Extensions.Options;

namespace Praetura.LoanService.API.Features.Outbox.PublishOutboxMessages;

/// <summary>
/// Publishes outbox messages once at startup and then on a fixed interval.
/// </summary>
public class OutboxPublishingService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxPublishingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(options.Value.IntervalSeconds);

        logger.LogInformation(
            "Outbox publishing started. Interval: {Interval}, batch size: {BatchSize}.",
            interval,
            options.Value.BatchSize);

        try
        {
            await RunOnceAsync(stoppingToken);

            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunOnceAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }

        logger.LogInformation("Outbox publishing stopped.");
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            // The DbContext is scoped but this service is a singleton, so each run gets its own scope.
            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IPublishOutboxMessagesHandler>();

            await handler.HandleAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // An unexpected failure (for example the database being unavailable) must not end the loop.
            logger.LogError(ex, "Outbox publishing run failed. It will be retried at the next interval.");
        }
    }
}

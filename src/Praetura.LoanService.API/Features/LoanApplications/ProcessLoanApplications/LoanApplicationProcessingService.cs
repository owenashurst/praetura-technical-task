using Microsoft.Extensions.Options;

namespace Praetura.LoanService.API.Features.LoanApplications.ProcessLoanApplications;

public class LoanApplicationProcessingService(
    IServiceScopeFactory scopeFactory,
    IOptions<ProcessingOptions> options,
    ILogger<LoanApplicationProcessingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(options.Value.IntervalSeconds);
        
        logger.LogInformation(
            "Loan application processing started. Interval: {Interval}, batch size: {BatchSize}.",
            interval,
            options.Value.BatchSize);

        try
        {
            // Run straight away so loan applications do not wait a full interval after startup.
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

        logger.LogInformation("Loan application processing stopped.");
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IProcessPendingLoanApplicationsHandler>();

            await handler.HandleAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Loan application processing run failed. It will be retried at the next interval.");
        }
    }
}

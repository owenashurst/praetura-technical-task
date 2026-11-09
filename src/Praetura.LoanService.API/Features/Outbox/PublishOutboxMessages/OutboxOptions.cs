namespace Praetura.LoanService.API.Features.Outbox.PublishOutboxMessages;

/// <summary>
/// Settings for the background job that publishes outbox messages.
/// </summary>
public class OutboxOptions
{
    /// <summary>Seconds to wait between runs.</summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>Maximum number of messages published in one run.</summary>
    public int BatchSize { get; set; } = 50;
}

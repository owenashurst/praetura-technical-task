using Microsoft.Extensions.Options;
using Praetura.LoanService.Domain.Messaging;
using Praetura.LoanService.Domain.Repositories;

namespace Praetura.LoanService.API.Features.Outbox.PublishOutboxMessages;

public class PublishOutboxMessagesHandler(
    IOutboxRepository outboxRepository,
    IMessagePublisher publisher,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<PublishOutboxMessagesHandler> logger) : IPublishOutboxMessagesHandler
{
    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        var messages = await outboxRepository.GetUnpublishedAsync(options.Value.BatchSize, cancellationToken);
        if (messages.Count == 0)
        {
            logger.LogDebug("No unpublished outbox messages.");
            return 0;
        }

        logger.LogInformation("Publishing {Count} outbox message(s).", messages.Count);

        var published = 0;
        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Publish first, then mark. A crash in between means the message is published again and never lost,
                // so consumers must tolerate duplicates (at-least-once delivery).
                await publisher.PublishAsync(message, cancellationToken);
                
                await outboxRepository.MarkPublishedAsync(
                    message.Id,
                    timeProvider.GetUtcNow().UtcDateTime,
                    cancellationToken);

                published++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(
                    ex,
                    "Failed to publish outbox message {MessageId} of type {MessageType}. It stays unpublished and will be retried.",
                    message.Id,
                    message.Type);
            }
        }

        return published;
    }
}

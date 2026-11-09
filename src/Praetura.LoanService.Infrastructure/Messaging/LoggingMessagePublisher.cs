using Microsoft.Extensions.Logging;
using Praetura.LoanService.Domain.Entities;
using Praetura.LoanService.Domain.Messaging;

namespace Praetura.LoanService.Infrastructure.Messaging;

public class LoggingMessagePublisher(ILogger<LoggingMessagePublisher> logger) : IMessagePublisher
{
    public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Published message {MessageId} of type {MessageType}: {Payload}",
            message.Id,
            message.Type,
            message.Payload);

        return Task.CompletedTask;
    }
}

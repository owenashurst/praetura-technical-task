using Praetura.LoanService.Domain.Entities;

namespace Praetura.LoanService.Domain.Messaging;

public interface IMessagePublisher
{
    /// <summary>
    /// Sends an outbox message
    /// </summary>
    /// <param name="message"><see cref="OutboxMessage"/>The message to publish</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
}

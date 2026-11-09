namespace Praetura.LoanService.API.Features.Outbox.PublishOutboxMessages;

public interface IPublishOutboxMessagesHandler
{
    /// <summary>
    /// Publishes one batch of unpublished outbox messages and marks each as published.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/>The number of published messages</returns>
    Task<int> HandleAsync(CancellationToken cancellationToken);
}

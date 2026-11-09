using Praetura.LoanService.Domain.Entities;

namespace Praetura.LoanService.Domain.Repositories;

public interface IOutboxRepository
{
    /// <summary>
    /// Retrieves a specified batch of messages that have not been published yet.
    /// </summary>
    /// <param name="batchSize"><see cref="int"/>The batch size to retrieve</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IReadOnlyList{T}"/>A list of messages awaiting to be published</returns>
    Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a message as published at the specified time.
    /// </summary>
    /// <param name="messageId"><see cref="Guid"/>The Id of the message</param>
    /// <param name="publishedAt"><see cref="DateTime"/>The Datetime the message was published at</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task MarkPublishedAsync(Guid messageId, DateTime publishedAt, CancellationToken cancellationToken);
}

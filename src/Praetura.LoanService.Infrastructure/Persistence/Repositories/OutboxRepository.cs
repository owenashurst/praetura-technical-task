using Microsoft.EntityFrameworkCore;
using Praetura.LoanService.Domain.Entities;
using Praetura.LoanService.Domain.Repositories;

namespace Praetura.LoanService.Infrastructure.Persistence.Repositories;

public class OutboxRepository(LoanServiceDbContext dbContext) : IOutboxRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken cancellationToken) =>
        await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(m => m.PublishedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    public Task MarkPublishedAsync(Guid messageId, DateTime publishedAt, CancellationToken cancellationToken) =>
        dbContext.OutboxMessages
            .Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.PublishedAt, publishedAt), cancellationToken);
}

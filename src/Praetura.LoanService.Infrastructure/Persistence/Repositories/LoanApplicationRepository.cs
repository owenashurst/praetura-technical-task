using Microsoft.EntityFrameworkCore;
using Praetura.LoanService.Domain.Entities;
using Praetura.LoanService.Domain.Enums;
using Praetura.LoanService.Domain.Repositories;

namespace Praetura.LoanService.Infrastructure.Persistence.Repositories;

public class LoanApplicationRepository(LoanServiceDbContext dbContext) : ILoanApplicationRepository
{
    public async Task AddAsync(LoanApplication loanApplication, CancellationToken cancellationToken)
    {
        dbContext.LoanApplications.Add(loanApplication);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.LoanApplications
            .AsNoTracking()
            .Include(a => a.DecisionLog)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<LoanApplication>> GetPendingAsync(int batchSize, CancellationToken cancellationToken) =>
        await dbContext.LoanApplications
            .AsNoTracking()
            .Where(a => a.Status == LoanApplicationStatus.Pending)
            .OrderBy(a => a.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    public async Task SaveDecisionAsync(
        LoanApplication loanApplication,
        IReadOnlyCollection<DecisionLogEntry> decisionLog,
        CancellationToken cancellationToken)
    {
        try
        {
            // The loan application was read untracked. Attach it and mark only the fields the review changes.
            dbContext.LoanApplications.Attach(loanApplication);
            
            var entry = dbContext.Entry(loanApplication);
            entry.Property(a => a.Status).IsModified = true;
            entry.Property(a => a.ReviewedAt).IsModified = true;
            
            dbContext.DecisionLogEntries.AddRange(decisionLog);
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            dbContext.ChangeTracker.Clear();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Praetura.LoanService.Domain.Entities;
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
}
